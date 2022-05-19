using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oowada.NetworkingFramework.Common;


public class PositionSynchronizationMessage : BaseMessage
{
    public string Name { get; set; } = "Player";
    public int X { get; set; } = 0;
    public int Z { get; set; } = 0;

    public PositionSynchronizationMessage(string name, int x, int z)
    {
        Name = name;
        X = x;
        Z = z;
    }

}

