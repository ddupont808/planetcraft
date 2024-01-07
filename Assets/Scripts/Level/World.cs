using UnityEngine;

public class World : MonoBehaviour
{
    public GameObject chunk;
    public Chunk[,,] chunks;
    public int chunkSize = 16;
    // public byte[,,] data;
    public int worldX = 16;
    public int worldY = 16;
    public int worldZ = 16;

    public int cx = 0;
    public int cy = 0;
    public int cz = 0;
    public int region = 0;

    public VoxelGenerator voxelGenerator;

    public enum Space
    {
        Region, SubRegion
    }

    public void Initialize()
    {
        ModLoader modLoader = FindObjectOfType<ModLoader>();
        voxelGenerator = new VoxelGenerator();
        voxelGenerator.Initialize(chunkSize, modLoader.blocksRegistry);
         
        /*
        data = new byte[worldX, worldY, worldZ];

        for(int x = 0; x < worldZ; x++)
        {
            for(int z = 0; z < worldZ; z++)
            {
                for(int y = 0; y < worldY; y++)
                {
                    data[x, y, z] = 
                        y < 6 ? (byte)0x01 : // stone
                        y < 7 ? (byte)0x03 : // dirt
                        y < 8 ? (byte)0x02 : // grass
                                (byte)0x00;
                }
            }
        }

        for (int x = 0; x < worldZ; x++)
        {
            for (int i = 33; i < 35; i++)
            {
                data[x, 30, i] = (byte)0x04;
                data[i, 30, x] = (byte)0x04;
            }
        }
        */

        chunks = new Chunk[Mathf.FloorToInt(worldX / chunkSize), Mathf.FloorToInt(worldY / chunkSize), Mathf.FloorToInt(worldZ / chunkSize)];
    }

    private void OnDrawGizmos()
    {
        Vector3[] corners = new Vector3[]
        {
            // bottom 
            PlanetMath.RegionToSphere(new Vector3(cx * chunkSize, cy * chunkSize, cz * chunkSize), region),
            PlanetMath.RegionToSphere(new Vector3(cx * chunkSize + worldX, cy * chunkSize, cz * chunkSize), region),
            PlanetMath.RegionToSphere(new Vector3(cx * chunkSize + worldX, cy * chunkSize, cz * chunkSize + worldZ), region),
            PlanetMath.RegionToSphere(new Vector3(cx * chunkSize, cy * chunkSize, cz * chunkSize + worldZ), region),
            // top 
            PlanetMath.RegionToSphere(new Vector3(cx * chunkSize, cy * chunkSize + worldY, cz * chunkSize), region),
            PlanetMath.RegionToSphere(new Vector3(cx * chunkSize + worldX, cy * chunkSize + worldY, cz * chunkSize), region),
            PlanetMath.RegionToSphere(new Vector3(cx * chunkSize + worldX, cy * chunkSize + worldY, cz * chunkSize + worldZ), region),
            PlanetMath.RegionToSphere(new Vector3(cx * chunkSize, cy * chunkSize + worldY, cz * chunkSize + worldZ), region),
        };

        Gizmos.color = Color.yellow;

        Gizmos.DrawLineList(new Vector3[]
        {
            // bottom
            corners[0], corners[1],
            corners[1], corners[2],
            corners[2], corners[3],
            corners[3], corners[0],
            // top
            corners[4], corners[5],
            corners[5], corners[6],
            corners[6], corners[7],
            corners[7], corners[4],
            // sides
            corners[0], corners[4],
            corners[1], corners[5],
            corners[2], corners[6],
            corners[3], corners[7],
        });
    }

    public void GenColumn(int x, int z)
    {
        for (int y = 0; y < chunks.GetLength(1); y++)
        {
            if (chunks[x, y, z] != null) continue;
            var distortion = PlanetMath.GetPoints(cx + x, cy + y, cz + z, region);

            var left = (distortion.s100 - distortion.s000).normalized;
            var up = (distortion.s010 - distortion.s000).normalized;
            var forward = (distortion.s001 - distortion.s000).normalized;
            var matrix = new Matrix4x4(left, up, forward, Vector3.zero);

            GameObject chunkObj = Instantiate(chunk, distortion.s000, Quaternion.identity);
            var newChunk = chunkObj.GetComponent("Chunk") as Chunk;

            newChunk.world = this;
            newChunk.chunkSize = chunkSize;
            newChunk.chunkX = x * chunkSize;
            newChunk.chunkY = y * chunkSize;
            newChunk.chunkZ = z * chunkSize;
            newChunk.cx = cx;
            newChunk.cy = cy;
            newChunk.cz = cz;
            newChunk.chunkRegion = region;
            newChunk.matrix = matrix;

            chunks[x, y, z] = newChunk;
        }
    }

    public void UnloadColumn(int x, int z)
    {
        for (int y = 0; y < chunks.GetLength(1); y++)
        {
            if (chunks[x, y, z] == null) continue;
            Object.Destroy(chunks[x, y, z].gameObject);
        }
    }

    public byte Block(int x, int y, int z, Space space = Space.SubRegion)
    {
        if (space == Space.Region)
        {
            x -= cx * chunkSize;
            y -= cy * chunkSize;
            z -= cz * chunkSize;
        }

        if (x < worldX && x >= 0 && y < worldY && y >= 0 && z < worldZ && z >= 0)
        {
            int chunkX = Mathf.FloorToInt(x / chunkSize);
            int chunkY = Mathf.FloorToInt(y / chunkSize);
            int chunkZ = Mathf.FloorToInt(z / chunkSize);

            var chunk = chunks[chunkX, chunkY, chunkZ];
            if (chunk != null) return chunk.Block(x - chunk.chunkX, y - chunk.chunkY, z - chunk.chunkZ);
        }
        return GenerateBlock(x, y, z);
    }

    public void Block(int x, int y, int z, byte newValue)
    {
        if (x >= worldX || x < 0 || y >= worldY || y < 0 || z >= worldZ || z < 0)
        {
            return;
        }

        // data[x, y, z] = newValue;
    }

    public byte GenerateBlock(int x, int y, int z)
    {
        /*
        if (x >= worldX || x < 0 || y >= worldY || y < 0 || z >= worldZ || z < 0)
            return (byte)1;
        */

        if(y >= 40 && y <= 54)
        {
            if (x == 2 || x == 3 || z == 3 || z == 2)
                return y < 52 ? (byte)0x01 : // stone
                       y < 53 ? (byte)0x03 : // dirt
                       y < 54 ? (byte)0x02 : // grass
                               (byte)0x00;
        }

        return y < 6 ? (byte)0x01 : // stone
               y < 7 ? (byte)0x03 : // dirt
               y < 8 ? (byte)0x02 : // grass
                       (byte)0x00;
    }
}