using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Cell
{
    // currently alive
    public bool isAlive;
    // place to store decision on whether the cell will be alive tomorrow
    public bool willBeAlive;
    // position x
    public int x;
    // position y
    public int y;
    public GameObject repr;

    public Cell(int x, int y, bool isAlive = true)
    {
        this.x = x;
        this.y = y;
        this.isAlive = isAlive;
        this.willBeAlive = isAlive;
        this.repr = null;
    }
}


public struct World
{
    public readonly int width;
    public readonly int height;
    public Cell[][] cells;
    public delegate GameObject ReprConstructor(Cell cell);

    public World(int width, int height, ReprConstructor repr_constructor)
    {
        this.width = width;
        this.height = height;
        this.cells = new Cell[height][];
        for (int y_pos = 0; y_pos < height; y_pos++)
        {
            Cell[] cells_line = new Cell[width];
            for (int x_pos = 0; x_pos < width; x_pos++)
            {
                Cell cell = new Cell(x: x_pos, y: y_pos, isAlive: false);
                cell.repr = repr_constructor(cell);
                cells_line[x_pos] = cell;
            }
            this.cells[y_pos] = cells_line;
        }
    }
}

class GOLRunner
{
    public World world;
    public GOLRunner(int width, int height, World.ReprConstructor repr_constructor)
    {
        this.world = new World(width, height, repr_constructor);
    }

    public int count_alive_around(int y, int x)
    {
        int alives = 0;

        // handy aliases
        int width = world.width;
        int height = world.height;
        ref Cell[][] cells = ref world.cells;
        ref Cell current_cell = ref world.cells[y][x];

        // Checking 8 corner tiles around with respect to the tor topology
        if (cells[(y + 1) % height][x].isAlive)
        {
            alives++;
        }
        if (cells[(y - 1 + height) % height][x].isAlive)
        {
            alives++;
        }
        if (cells[y][(x + 1) % width].isAlive)
        {
            alives++;
        }
        if (cells[y][(x - 1 + width) % width].isAlive)
        {
            alives++;
        }
        if (cells[(y - 1 + height) % height][(x - 1 + width) % width].isAlive)
        {
            alives++;
        }
        if (cells[(y + 1) % height][(x + 1) % width].isAlive)
        {
            alives++;
        }
        if (cells[(y - 1 + height) % height][(x + 1) % width].isAlive)
        {
            alives++;
        }
        if (cells[(y + 1) % height][(x - 1 + width) % width].isAlive)
        {
            alives++;
        }
        return alives;
    }

    private void prepare()
    {
        for (int lineno = 0; lineno < world.height; lineno++)
        {
            for (int colno = 0; colno < world.width; colno++)
            {
                int alives = count_alive_around(lineno, colno);
                ref Cell current_cell = ref world.cells[lineno][colno];

                if (!current_cell.isAlive & alives == 3)
                {
                    // Breeding case
                    current_cell.willBeAlive = true;
                }
                else if (alives < 2 | alives > 3)
                {
                    // {Over,under}population case
                    current_cell.willBeAlive = false;
                }
                else
                {
                    // Default case
                    current_cell.willBeAlive = current_cell.isAlive;
                }
            }
        }
    }

    private void commit()
    {
        for (int lineno = 0; lineno < this.world.height; lineno++)
        {
            for (int colno = 0; colno < this.world.width; colno++)
            {
                ref var current_cell = ref this.world.cells[lineno][colno];
                current_cell.isAlive = current_cell.willBeAlive;
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
    private float timeToUpdate = 0;
    public GameObject cell_repr_template;
    private GOLRunner simulator;
    public int howManyPreinit = 100;

    void Start()
    {
        int width = 160;
        int height = 100;
        int counter = this.howManyPreinit;
        this.cell_repr_template = Resources.Load("Prefabs/cell") as GameObject;

        this.simulator = new GOLRunner(
            width, height,
            repr_constructor: (Cell cell) =>
            {
                GameObject cell_repr = Instantiate(
                    cell_repr_template,
                    position: new Vector3(cell.x, cell.y, 0),
                    rotation: Quaternion.identity
                );
                return cell_repr;
            }
        );

        while (counter > 0)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            simulator.world.cells[y][x].isAlive = true;
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
            for (int pos_y = 0; pos_y < simulator.world.height; pos_y++)
            {
                for (int pos_x = 0; pos_x < simulator.world.width; pos_x++)
                {
                    var this_cell = simulator.world.cells[pos_y][pos_x];
                    var renderer = this_cell.repr.GetComponent<SpriteRenderer>();
                    renderer.enabled = this_cell.isAlive;
                }
            }
        };
    }
}