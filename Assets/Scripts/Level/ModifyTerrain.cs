using UnityEngine;
using System.Collections;

public class ModifyTerrain : MonoBehaviour
{
    GameObject cameraGO;

    // Use this for initialization
    void Start()
    {

        cameraGO = GameObject.FindGameObjectWithTag("MainCamera");

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            ReplaceBlockCenter(16, 0);
        }

        if (Input.GetMouseButtonDown(1))
        {
            AddBlockCenter(16, (byte)Random.Range(1, 19));

        }

    }

    public void ReplaceBlockCenter(float range, byte block)
    {
        //Replaces the block directly in front of the player

        Ray ray = new Ray(cameraGO.transform.position, cameraGO.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {

            if (hit.distance < range)
            {
                ReplaceBlockAt(hit, block);
            }
        }

    }

    public void AddBlockCenter(float range, byte block)
    {
        //Adds the block specified directly in front of the player

        Ray ray = new Ray(cameraGO.transform.position, cameraGO.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {

            if (hit.distance < range)
            {
                AddBlockAt(hit, block);
            }
            Debug.DrawLine(ray.origin, ray.origin + (ray.direction * hit.distance), Color.green, 2);
        }

    }

    public void ReplaceBlockCursor(byte block)
    {
        //Replaces the block specified where the mouse cursor is pointing

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {

            ReplaceBlockAt(hit, block);

        }

    }

    public void AddBlockCursor(byte block)
    {
        //Adds the block specified where the mouse cursor is pointing

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {

            AddBlockAt(hit, block);
            Debug.DrawLine(ray.origin, ray.origin + (ray.direction * hit.distance), Color.green, 2);
        }

    }

    public void ReplaceBlockAt(RaycastHit hit, byte block)
    {
        //removes a block at these impact coordinates, you can raycast against the terrain and call this with the hit.point
        Vector3 position = hit.point;
        position += (hit.normal * -0.5f);

        SetBlockAt(position, block, hit.transform.GetComponent<Chunk>().world);
    }

    public void AddBlockAt(RaycastHit hit, byte block)
    {
        //adds the specified block at these impact coordinates, you can raycast against the terrain and call this with the hit.point
        Vector3 position = hit.point;
        position += (hit.normal * 0.5f);

        SetBlockAt(position, block, hit.transform.GetComponent<Chunk>().world);

    }

    public void SetBlockAt(Vector3 position, byte block, World world)
    {
        var blockPos = PlanetMath.PointToChunkCoords(position);
        Debug.Log($"setting {blockPos.blockX}, {blockPos.blockY}, {blockPos.blockZ}");

        //sets the specified block at these coordinates

        int x = blockPos.blockX; // Mathf.RoundToInt(position.x);
        int y = blockPos.blockY; // Mathf.RoundToInt(position.y);
        int z = blockPos.blockZ; // Mathf.RoundToInt(position.z);

        SetBlockAt(x, y, z, block, world);
    }

    public void SetBlockAt(int x, int y, int z, byte block, World world)
    {
        //adds the specified block at these coordinates

        print("Adding: " + x + ", " + y + ", " + z);

        x -= world.cx * world.chunkSize;
        y -= world.cy * world.chunkSize;
        z -= world.cz * world.chunkSize;

        int updateX = Mathf.FloorToInt(x / world.chunkSize);
        int updateY = Mathf.FloorToInt(y / world.chunkSize);
        int updateZ = Mathf.FloorToInt(z / world.chunkSize);

        var chunk = world.chunks[updateX, updateY, updateZ];
        if (chunk != null)
        {
            chunk.SetBlock(x - updateX * world.chunkSize, y - updateY * world.chunkSize, z - updateZ * world.chunkSize, block, true);
        }

        // world.Block(x, y, z, block);
        // UpdateChunkAt(x, y, z, block, world);
    }

    // TODO: move chunk neighbor updates elsewhere
    public void UpdateChunkAt(int x, int y, int z, byte block, World world)
    {       //To do: add a way to just flag the chunk for update and then it updates in lateupdate
            //Updates the chunk containing this block

        int updateX = Mathf.FloorToInt(x / world.chunkSize);
        int updateY = Mathf.FloorToInt(y / world.chunkSize);
        int updateZ = Mathf.FloorToInt(z / world.chunkSize);

        print("Updating: " + updateX + ", " + updateY + ", " + updateZ);


        world.chunks[updateX, updateY, updateZ].update = true;

        if (x - (world.chunkSize * updateX) == 0 && updateX != 0)
        {
            world.chunks[updateX - 1, updateY, updateZ].update = true;
        }

        if (x - (world.chunkSize * updateX) == world.chunkSize - 1 && updateX != world.chunks.GetLength(0) - 1)
        {
            world.chunks[updateX + 1, updateY, updateZ].update = true;
        }

        if (y - (world.chunkSize * updateY) == 0 && updateY != 0)
        {
            world.chunks[updateX, updateY - 1, updateZ].update = true;
        }

        if (y - (world.chunkSize * updateY) == world.chunkSize - 1 && updateY != world.chunks.GetLength(1) - 1)
        {
            world.chunks[updateX, updateY + 1, updateZ].update = true;
        }

        if (z - (world.chunkSize * updateZ) == 0 && updateZ != 0)
        {
            world.chunks[updateX, updateY, updateZ - 1].update = true;
        }

        if (z - (world.chunkSize * updateZ) == world.chunkSize - 1 && updateZ != world.chunks.GetLength(2) - 1)
        {
            world.chunks[updateX, updateY, updateZ + 1].update = true;
        }

    }

}
