using UnityEngine;
using Binders;


namespace Binders
{
    public delegate R ReprConstructor<R>(Cell cell) where R : ICellRepresentation;
}


public enum CellState
{
    Alive, Dead
}


public interface ICellRepresentation
{
    CellState current_state { get; set; }
}


public struct Cell
{
    // currently alive
    public CellState current_state;
    // place to store decision on whether the cell will be alive tomorrow
    public CellState next_state;
    // position x
    public int x;
    // position y
    public int y;

    public Cell(int x, int y, CellState current_state = CellState.Alive)
    {
        this.x = x;
        this.y = y;
        this.current_state = current_state;
        this.next_state = current_state;
    }
}

public class World<R> where R : ICellRepresentation
{
    public readonly int width;
    public readonly int height;

    private struct CompoundCells<R>
    {
        public Cell[][] cells;
        public R[][] reprs;
    }
    private CompoundCells<R> cells;

    public World(int width, int height, ReprConstructor<R> rconstructor)
    {
        this.width = width;
        this.height = height;
        this.cells = new CompoundCells<R>();

        this.cells.cells = new Cell[height][];
        this.cells.reprs = new R[height][];

        for (int y_pos = 0; y_pos < height; y_pos++)
        {
            var cells_line = new Cell[width];
            var reprs_line = new R[width];
            for (int x_pos = 0; x_pos < width; x_pos++)
            {
                var cell = new Cell(x: x_pos, y: y_pos, current_state: CellState.Dead);
                var cell_repr = rconstructor(cell);
                cells_line[x_pos] = cell;
                reprs_line[x_pos] = cell_repr;
            }
            this.cells.cells[y_pos] = cells_line;
            this.cells.reprs[y_pos] = reprs_line;
        }
    }

    public ref Cell[][] get_simulation_cells()
    {
        return ref this.cells.cells;
    }

    public void set_cell_state(int x, int y, CellState state)
    {
        this.cells.cells[y][x].current_state = state;
        this.cells.reprs[y][x].current_state = state;
    }

    public void commit_cell_state(int x, int y)
    {
        this.set_cell_state(x, y, this.cells.cells[y][x].next_state);
    }
}

class GOLRunner<R> where R : ICellRepresentation
{
    public World<R> world;
    public GOLRunner(int width, int height, ReprConstructor<R> rconstructor)
    {
        world = new World<R>(width, height, rconstructor);
    }

    public int count_alive_around(int y, int x)
    {
        int alives = 0;

        // handy aliases
        int width = world.width;
        int height = world.height;
        ref readonly Cell[][] cells = ref world.get_simulation_cells();
        ref Cell current_cell = ref cells[y][x];

        // Checking 8 corner tiles around with respect to the tor topology
        if (cells[(y + 1) % height][x].current_state == CellState.Alive)
        {
            alives++;
        }
        if (cells[(y - 1 + height) % height][x].current_state == CellState.Alive)
        {
            alives++;
        }
        if (cells[y][(x + 1) % width].current_state == CellState.Alive)
        {
            alives++;
        }
        if (cells[y][(x - 1 + width) % width].current_state == CellState.Alive)
        {
            alives++;
        }
        if (cells[(y - 1 + height) % height][(x - 1 + width) % width].current_state == CellState.Alive)
        {
            alives++;
        }
        if (cells[(y + 1) % height][(x + 1) % width].current_state == CellState.Alive)
        {
            alives++;
        }
        if (cells[(y - 1 + height) % height][(x + 1) % width].current_state == CellState.Alive)
        {
            alives++;
        }
        if (cells[(y + 1) % height][(x - 1 + width) % width].current_state == CellState.Alive)
        {
            alives++;
        }
        return alives;
    }

    private void prepare()
    {
        ref readonly Cell[][] cells = ref world.get_simulation_cells();
        for (int lineno = 0; lineno < world.height; lineno++)
        {
            for (int colno = 0; colno < world.width; colno++)
            {
                int alives = count_alive_around(lineno, colno);
                ref Cell current_cell = ref cells[lineno][colno];

                if (!(current_cell.current_state == CellState.Alive) & alives == 3)
                {
                    // Breeding case
                    current_cell.next_state = CellState.Alive;
                }
                else if (alives < 2 | alives > 3)
                {
                    // {Over,under}population case
                    current_cell.next_state = CellState.Dead;
                }
                else
                {
                    // Default case
                    current_cell.next_state = current_cell.current_state;
                }
            }
        }
    }

    private void commit()
    {
        ref readonly Cell[][] cells = ref world.get_simulation_cells();
        for (int lineno = 0; lineno < this.world.height; lineno++)
        {
            for (int colno = 0; colno < this.world.width; colno++)
            {
                world.commit_cell_state(x: colno, y: lineno);
            }
        }
    }

    public void simulation_step()
    {
        prepare();
        commit();
    }
}


public class CellRepresentation : ICellRepresentation
{
    private GameObject _unity_object;
    private GameObject _unity_object_template;
    private Cell _cell_object;
    public CellState current_state
    {
        get
        {
            return this._cell_object.current_state;
        }
        set
        {
            if (value == CellState.Alive)
            {
                if (this._unity_object == null)
                {
                    this._unity_object = Object.Instantiate(
                        this._unity_object_template,
                        new Vector3(this._cell_object.x, this._cell_object.y, 0),
                        rotation: Quaternion.identity
                    );
                }
                else
                {
                    this._unity_object.GetComponent<SpriteRenderer>().enabled = true;
                }
            }
            else
            {
                if (this._unity_object != null)
                {
                    this._unity_object.GetComponent<SpriteRenderer>().enabled = false;
                }
            }
        }
    }
    public CellRepresentation(Cell cell_object, GameObject template)
    {
        this._unity_object = null;
        this._unity_object_template = template;
        this._cell_object = cell_object;
    }
}


public class GameIniter : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera mainCamera;
    public float refreshRate;
    public int preinitCount;

    private float timeToUpdate = 0;
    private GOLRunner<CellRepresentation> simulator;

    void Start()
    {
        int counter = this.preinitCount;

        int height = Mathf.CeilToInt(mainCamera.orthographicSize * 2);
        int width = height * mainCamera.pixelWidth / mainCamera.pixelHeight;
        mainCamera.transform.position = new Vector3((int)width / 2, (int)height / 2, -10);

        GameObject cell_repr_template = (GameObject)Resources.Load("Prefabs/cell");

        simulator = new GOLRunner<CellRepresentation>(
            width: width,
            height: height,
            rconstructor: (Cell cell) => (new CellRepresentation(cell, cell_repr_template))
        );

        while (counter > 0)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            simulator.world.set_cell_state(x, y, CellState.Alive);
            counter--;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float current_time = Time.time;
        if (Time.time >= this.timeToUpdate)
        {
            timeToUpdate = current_time + 1f / refreshRate;
            simulator.simulation_step();
        };
    }
}