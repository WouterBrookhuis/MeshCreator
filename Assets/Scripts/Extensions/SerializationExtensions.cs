using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SerializationExtensions
{
    public static void Write(this BinaryWriter bw, Vector2 value)
    {
        bw.Write(value.x);
        bw.Write(value.y);
    }

    public static void Write(this BinaryWriter bw, Vector3 value)
    {
        bw.Write(value.x);
        bw.Write(value.y);
        bw.Write(value.z);
    }

    public static void Write(this BinaryWriter bw, Vector4 value)
    {
        bw.Write(value.x);
        bw.Write(value.y);
        bw.Write(value.z);
        bw.Write(value.w);
    }

    public static Vector2 ReadVector2(this BinaryReader br)
    {
        float x = br.ReadSingle();
        float y = br.ReadSingle();
        return new Vector2(x, y);
    }

    public static Vector3 ReadVector3(this BinaryReader br)
    {
        float x = br.ReadSingle();
        float y = br.ReadSingle();
        float z = br.ReadSingle();
        return new Vector3(x, y, z);
    }

    public static Vector4 ReadVector4(this BinaryReader br)
    {
        float x = br.ReadSingle();
        float y = br.ReadSingle();
        float z = br.ReadSingle();
        float w = br.ReadSingle();
        return new Vector4(x, y, z, w);
    }
}
