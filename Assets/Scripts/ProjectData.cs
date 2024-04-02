using System;
using System.Windows.Forms;
using UnityEngine;

[Serializable]
public class ProjectData
{
    public string meshURL;
    public int frames;
    public Vector2Int cellSize;
    public Vector2 rotation;
    public Vector3 scale;
}
