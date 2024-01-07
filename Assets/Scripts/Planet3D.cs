using UnityEngine;

public class Planet3D : MonoBehaviour
{
    public Transform player;

    int old_cx = -1;
    int old_cy = -1;
    int old_cz = -1;

    public World[,] subregions;
    public GameObject subregionPrefab;

    int surfaceSize = 256;
    public Texture2D surfaceMask;
    Color32[] surfaceMask_data;
    bool surface_dirty = false;

    private void Start()
    {
        subregions = new World[4, 4];

        surfaceMask = new Texture2D(surfaceSize * 3, surfaceSize * 2, TextureFormat.R8, false);
        surfaceMask.filterMode = FilterMode.Bilinear;
        surfaceMask_data = new Color32[surfaceSize * 3 * surfaceSize * 2];
        surfaceMask.SetPixels32(surfaceMask_data);
        surfaceMask.Apply();

        Shader.SetGlobalTexture("_planet_SurfaceMask", surfaceMask);
    }

    private void Update()
    {
        // Load chunks around camera
        {
            var cubized = PlanetMath.PointToChunkCoords(player.position);

            if (cubized.region >= 0)
            {
                if (cubized.x != old_cx || cubized.y != old_cy || cubized.z != old_cz)
                {
                    LoadChunk(cubized.x, cubized.y, cubized.z, cubized.region);

                    Debug.Log($"loading {cubized.x}, {cubized.y}, {cubized.z} : {cubized.region}");

                    old_cx = cubized.x;
                    old_cy = cubized.y;
                    old_cz = cubized.z;
                }
            }
        }

        // Update masks
        {
            if(surface_dirty)
            {
                surface_dirty = false;
                surfaceMask.SetPixels32(surfaceMask_data);
                surfaceMask.Apply();
            }
        }
    }

    void LoadChunk(int cx, int cy, int cz, int region)
    {
        int subregion_x = cx / 64; // 2048 / 32
        int subregion_y = 3; // cy / 8;
        int subregion_z = cz / 64;

        if (subregion_x < 0 || subregion_x >= 4 || subregion_z < 0 || subregion_z >= 4 || region != 6)
            return;

        World world;
        if (subregions[subregion_x, subregion_z] == null)
        {
            var subregion = Instantiate(subregionPrefab, Vector3.zero, Quaternion.identity);
            world = subregion.GetComponent<World>();
            world.cx = subregion_x * 64;
            world.cy = subregion_y * 8;
            world.cz = subregion_z * 64;
            world.region = region;
            world.Initialize();
            subregions[subregion_x, subregion_z] = world;
        }
        else world = subregions[subregion_x, subregion_z];

        int ocx = cx - world.cx;
        int lcy = cy - world.cy;
        int ocz = cz - world.cz;

        for (int lcx = ocx - 3; lcx < ocx + 3; lcx++)
        {
            for (int lcz = ocz - 3; lcz < ocz + 3; lcz++)
            {
                if (lcx >= 0 && lcy >= 0 && lcz >= 0 && lcx < world.chunks.GetLength(0) && lcy < world.chunks.GetLength(1) && lcz < world.chunks.GetLength(2))
                {
                    int side = 0;
                    int sm_x = (side % 3) * surfaceSize + lcx + world.cx;
                    int sm_y = ((int)(side / 3) == 0 ? 0 : surfaceSize) + lcz + world.cz;
                    surfaceMask_data[sm_x + sm_y * surfaceSize * 3] = new Color32(255, 0, 0, 0);
                    surface_dirty = true;

                    world.GenColumn(lcx, lcz);
                }/*
                else
                {
                    Debug.Log($"out of range {lcx}, {lcy}, {lcz}");
                }*/
            }
        }
    }
}