using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Player
{
    public string Name { get; set; } = "Player";
    public int X { get; set; } = 0;
    public int Z { get; set; } = 0;

    public void MoveForwad()
    {
        Z += 1;
    }

    public void MoveBack()
    {
        Z -= 1;
    }

    public void MoveLeft()
    {
        X -= 1;
    }

    public void MoveRight()
    {
        X += 1;
    }

    public override string ToString()
    {
        return "[" + X + ", " + Z + "]";
    }
}

