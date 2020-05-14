using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Binders;


namespace Binders
{
    public delegate T ReprConstructor<T>(Cell cell);
}


public enum CellState
{
    Alive, Dead
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

public class World
{
    public readonly int width;
    public readonly int height;

    private struct CompoundCells
    {
        public Cell[][] cells;
        public GameObject[][] reprs;
    }
    private CompoundCells cells;

    public World(int width, int height, ReprConstructor<GameObject> rconstructor)
    {
        this.width = width;
        this.height = height;
        this.cells = new CompoundCells();

        this.cells.cells = new Cell[height][];
        this.cells.reprs = new GameObject[height][];

        for (int y_pos = 0; y_pos < height; y_pos++)
        {
            var cells_line = new Cell[width];
            var reprs_line = new GameObject[width];
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
        if (state == CellState.Alive)
        {
            this.cells.reprs[y][x].GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            this.cells.reprs[y][x].GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public void commit_cell_state(int x, int y)
    {
        set_cell_state(x, y, this.cells.cells[y][x].next_state);
    }
}

class GOLRunner<CellReprType>
{
    public World world;
    public GOLRunner(int width, int height, ReprConstructor<GameObject> rconstructor)
    {
        world = new World(width, height, rconstructor);
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


public class GameIniter : MonoBehaviour
{
    // Start is called before the first frame update
    public float refreshRate;
    public int howManyPreinit = 100;

    private float timeToUpdate = 0;
    private GOLRunner<GameObject> simulator;

    void Start()
    {
        int width = 160;
        int height = 100;
        int counter = howManyPreinit;

        GameObject cell_repr_template = (GameObject)Resources.Load("Prefabs/cell");
        this.simulator = new GOLRunner<GameObject>(
            width: width,
            height: height,
            rconstructor: (Cell cell) => Instantiate(
                cell_repr_template,
                position: new Vector3(cell.x, cell.y, 0),
                rotation: Quaternion.identity
            )
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
    void FixedUpdate()
    {
        float current_time = Time.time;
        if (Time.time >= this.timeToUpdate)
        {
            timeToUpdate = current_time + 1f / refreshRate;
            simulator.simulation_step();
        };
    }
}