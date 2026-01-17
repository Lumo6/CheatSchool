using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine.AI;
using System.Collections;

public class LevelGenerator : MonoBehaviour
{
    public Transform PropsParent;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject wallDoorPrefab;
    public GameObject pillarPrefab;
    public GameObject pc_etuPrefab;
    public GameObject pc_profPrefab;
    public List<GameObject> windowWallPrefab;
    public GameObject doorPrefab;
    public GameObject patrolPointsPrefab;


    [Header("Room Size")]
    public int rows = 5;
    public int columns = 6;

    [Header("Student Desk Spacing")]
    public float rowSpacing = 1.5f;
    public float columnSpacing = 1.5f;


    [Header("Room Dimensions")]
    public int length = 3;
    public int width = 3;

    

    [Header("Player Desk")]
    private GameObject playerDesk;
    public Vector3 playerDeskOffset = new Vector3(0, 0, 1f);


    [Header("Lengths")]
    private Vector3 walllength;
    private Vector3 pillarlength;

    [Header("Cameras")]
    private List<Camera> Cameras;
    public Camera cameraPrefab;
    public float cameraOffset = 1f;

    [Header("World Grid")]
    private Vector2Int gridPos; // (0..2, 0..2)

    [Header("Characters")]
    public List<GameObject> rdmcharacters = new List<GameObject>();
    public List<GameObject> copycharacters = new List<GameObject>();
    public GameObject teacherCharacter;
    public GameObject Player;

    [Header("Random Obstacles")]
    public List<GameObject> obstaclePrefabs = new List<GameObject>();
    public int obstacleCount = 5;
    public float obstacleWallOffset = 0.5f;


    private List<GameObject> spawnedObjects = new List<GameObject>();
    enum WallSide { North, South, East, West }
    private HashSet<WallSide> outsideWalls = new HashSet<WallSide>();
    private WallSide doorWall;
    private float segmentLength;
    private float pillarSize;
    private GameObject floor;
    private NavMeshSurface navMeshSurface;
    private List<GameObject> copyTargetPositions = new List<GameObject>();
    private GameManager gm;

    void GenerateLevel()
    {
        // Clear previous level
        ClearLevel();

        PickGridPosition();

        GenerateFloor();

        GenerateWallsWithPillars();

        PlaceTeacherDesk();

        GenerateStudentDesks();

        GenerateRandomObstacles();

        navMeshSurface = floor.GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();

        // Now generate teacher
        GenerateTeacher();

        GenerateCameras();
        GeneratePlayer();

        
    }

    private void Start()
    {
        walllength = wallPrefab.GetComponent<MeshRenderer>().bounds.size;
        pillarlength = pillarPrefab.GetComponent<MeshRenderer>().bounds.size;
        segmentLength = walllength.z;
        pillarSize = pillarlength.z;
        gm = GameManager.Instance;
        GenerateLevel();
    }


    void ClearLevel()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();
    }

    void PickGridPosition()
    {
        gridPos = new Vector2Int(
            Random.Range(0, 3),
            Random.Range(0, 3)
        );

        Debug.Log($"Room grid position: {gridPos}");
    }


    void GenerateFloor()
    {
        int scale = Mathf.Max(width, length);

        Vector3 pos = new Vector3((
            segmentLength * width + pillarSize * (width + 1)) / 2,
            0,
            (segmentLength * length + pillarSize * (length + 1)) / 2
        );
        floor = Instantiate(
            floorPrefab,
            pos,
            Quaternion.identity,
            PropsParent
        );
        spawnedObjects.Add(floor);
        floor.transform.localScale = new Vector3(floor.transform.localScale.x * scale, 0, floor.transform.localScale.z * scale);

        pos.y = pillarlength.y;
        GameObject roof = Instantiate(
            floorPrefab,
            pos,
            Quaternion.identity,
            PropsParent
        );
        spawnedObjects.Add(roof);
        roof.transform.localScale = new Vector3(roof.transform.localScale.x * scale, 0, roof.transform.localScale.z * scale);
        NavMeshModifier navmodif = roof.AddComponent<NavMeshModifier>();
        navmodif.ignoreFromBuild = true;
    }

    void GenerateWallsWithPillars()
    {
        // Compute which walls are outside + where the door goes
        ComputeWallLogic();

        // 1. NORTH WALL
        for (int i = 0; i <= width; i++)
        {
            float xPos = i * (segmentLength + pillarSize);
            float zPos = length * (segmentLength + pillarSize);

            // Pillars
            Vector3 pillarPos = new Vector3(xPos, pillarlength.y / 2, zPos);
            spawnedObjects.Add(Instantiate(
                        pillarPrefab,
                        pillarPos,
                        Quaternion.identity,
                        PropsParent
                    )
                );

            // Wall segments
            if (i < width)
            {
                float wallXPos = xPos + pillarSize / 2 + segmentLength / 2;
                Vector3 wallPos = new Vector3(wallXPos, walllength.y / 2, zPos);

                GameObject wall;

                if (outsideWalls.Contains(WallSide.North) && Random.value < 0.3f)
                {
                    wall = Instantiate(
                        windowWallPrefab[Random.Range(0, windowWallPrefab.Count)],
                        wallPos,
                        Quaternion.Euler(0, 90, 0),
                        PropsParent
                    );
                }
                else if (doorWall == WallSide.North && i == width / 2)
                {
                    wall = Instantiate(
                        doorPrefab,
                        wallPos,
                        Quaternion.Euler(0, 90, 0),
                        PropsParent
                    );
                }
                else
                {
                    wall = Instantiate(
                        wallPrefab,
                        wallPos,
                        Quaternion.Euler(0, 90, 0),
                        PropsParent
                    );
                }

                spawnedObjects.Add(wall);
            }
        }

        // 2. SOUTH WALL
        for (int i = 0; i <= width; i++)
        {
            float xPos = i * (segmentLength + pillarSize);
            float zPos = 0;

            Vector3 pillarPos = new Vector3(xPos, pillarlength.y / 2, zPos);
            spawnedObjects.Add(Instantiate(
                        pillarPrefab,
                        pillarPos,
                        Quaternion.identity,
                        PropsParent
                    )
                );

            if (i < width)
            {
                float wallXPos = xPos + pillarSize / 2 + segmentLength / 2;
                Vector3 wallPos = new Vector3(wallXPos, walllength.y / 2, zPos);

                GameObject wall;

                if (outsideWalls.Contains(WallSide.South) && Random.value < 0.3f)
                {
                    wall = Instantiate(
                        windowWallPrefab[Random.Range(0, windowWallPrefab.Count)],
                        wallPos,
                        Quaternion.Euler(0, 90, 0),
                        PropsParent
                    );
                }
                else if (doorWall == WallSide.South && i == width / 2)
                {
                    wall = Instantiate(
                        doorPrefab,
                        wallPos,
                        Quaternion.Euler(0, 90, 0),
                        PropsParent
                    );
                }
                else
                {
                    wall = Instantiate(
                        wallPrefab,
                        wallPos,
                        Quaternion.Euler(0, 90, 0),
                        PropsParent
                    );
                }

                spawnedObjects.Add(wall);
            }
        }

        // 3. WEST WALL
        for (int i = 0; i <= length; i++)
        {
            float xPos = 0;
            float zPos = i * (segmentLength + pillarSize);

            if (i > 0 && i < length)
            {
                Vector3 pillarPos = new Vector3(xPos, pillarlength.y / 2, zPos);
                spawnedObjects.Add(Instantiate(
                        pillarPrefab,
                        pillarPos,
                        Quaternion.identity,
                        PropsParent
                    )
                );
            }

            if (i < length)
            {
                float wallZPos = zPos + pillarSize / 2 + segmentLength / 2;
                Vector3 wallPos = new Vector3(xPos, walllength.y / 2, wallZPos);

                GameObject wall;

                if (outsideWalls.Contains(WallSide.West) && Random.value < 0.3f)
                {
                    wall = Instantiate(
                        windowWallPrefab[Random.Range(0, windowWallPrefab.Count)],
                        wallPos,
                        Quaternion.identity,
                        PropsParent
                    );
                }
                else if (doorWall == WallSide.West && i == length / 2)
                {
                    wall = Instantiate(
                        doorPrefab,
                        wallPos,
                        Quaternion.identity,
                        PropsParent
                    );
                }
                else
                {
                    wall = Instantiate(
                        wallPrefab,
                        wallPos,
                        Quaternion.identity,
                        PropsParent
                    );
                }

                spawnedObjects.Add(wall);
            }
        }

        // 4. EAST WALL
        for (int i = 0; i <= length; i++)
        {
            float xPos = width * (segmentLength + pillarSize);
            float zPos = i * (segmentLength + pillarSize);

            if (i > 0 && i < length)
            {
                Vector3 pillarPos = new Vector3(xPos, pillarlength.y / 2, zPos);
                spawnedObjects.Add(Instantiate(
                        pillarPrefab,
                        pillarPos,
                        Quaternion.identity,
                        PropsParent
                    )
                );
            }

            if (i < length)
            {
                float wallZPos = zPos + pillarSize / 2 + segmentLength / 2;
                Vector3 wallPos = new Vector3(xPos, walllength.y / 2, wallZPos);

                GameObject wall;

                if (outsideWalls.Contains(WallSide.East) && Random.value < 0.3f)
                {
                    wall = Instantiate(
                        windowWallPrefab[Random.Range(0, windowWallPrefab.Count)],
                        wallPos,
                        Quaternion.identity,
                        PropsParent
                    );
                }
                else if (doorWall == WallSide.East && i == length / 2)
                {
                    wall = Instantiate(
                        doorPrefab,
                        wallPos,
                        Quaternion.identity,
                        PropsParent
                    );
                }
                else
                {
                    wall = Instantiate(
                        wallPrefab,
                        wallPos,
                        Quaternion.identity,
                        PropsParent
                    );
                }

                spawnedObjects.Add(wall);
            }
        }
    }


    void ComputeWallLogic()
    {
        outsideWalls.Clear();

        if (gridPos.x == 0) outsideWalls.Add(WallSide.West);
        if (gridPos.x == 2) outsideWalls.Add(WallSide.East);
        if (gridPos.y == 0) outsideWalls.Add(WallSide.South);
        if (gridPos.y == 2) outsideWalls.Add(WallSide.North);

        List<WallSide> insideWalls = new List<WallSide>
    {
        WallSide.North,
        WallSide.South,
        WallSide.East,
        WallSide.West
    };

        foreach (var w in outsideWalls)
            insideWalls.Remove(w);

        doorWall = insideWalls[Random.Range(0, insideWalls.Count)];
    }


    private void PlaceTeacherDesk()
    {

        float xPos = segmentLength;
        float zPos = segmentLength / 2;

        Vector3 teacherPos = new Vector3(xPos, 0, zPos);

        // Desk
        GameObject teacherDesk = Instantiate(
            pc_profPrefab,
            teacherPos,
            Quaternion.identity,
            PropsParent
        );
        spawnedObjects.Add(teacherDesk);
    }


    private void GenerateStudentDesks()
    {
        Renderer[] renderers = pc_etuPrefab.GetComponentsInChildren<Renderer>();

        Bounds deskBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            deskBounds.Encapsulate(renderers[i].bounds);

        float deskWidth = deskBounds.size.x;
        float deskDepth = deskBounds.size.z;

        float roomWidth = walllength.z * width + pillarlength.z * (width - 1);

        float gridWidth =
            columns * deskWidth +
            (columns - 1) * columnSpacing;

        float startX = (roomWidth - gridWidth) / 2f + deskWidth / 2f;

        List<GameObject> allDesks = new List<GameObject>();

        // Choose player desk index
        int playerDeskIndex = Random.Range(0, rows * columns);
        int currentIndex = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                float xPos = startX + col * (deskWidth + columnSpacing);
                float zPos = walllength.z + row * (deskDepth + rowSpacing);

                Vector3 position = new Vector3(xPos, 0, zPos);

                GameObject desk = Instantiate(
                    pc_etuPrefab,
                    position,
                    Quaternion.identity,
                    PropsParent
                );

                spawnedObjects.Add(desk);

                bool isPlayerDesk = currentIndex == playerDeskIndex;

                if (isPlayerDesk)
                {
                    playerDesk = desk;

                    BoxCollider colli = desk.GetComponent<BoxCollider>();
                    colli.isTrigger = true;

                    desk.AddComponent<PlayerDesk>();

                    Player.transform.position = position + playerDeskOffset;
                    Player.transform.rotation = Quaternion.Euler(0, 180, 0);
                    MakeDeskGlow(playerDesk, Color.green);
                }
                else
                {
                    allDesks.Add(desk);

                    // Spawn random student
                    GameObject characterPrefab =
                        rdmcharacters[Random.Range(0, rdmcharacters.Count)];

                    GameObject character = Instantiate(
                        characterPrefab,
                        position + new Vector3(0.2f, 0.8f, 0),
                        Quaternion.Euler(0, 180, 0),
                        PropsParent
                    );

                    character.transform.localScale = 2.0f * Vector3.one;
                }

                currentIndex++;
            }
        }

        // Assign copy targets only to non-player desks
        AssignCopyTargets(allDesks);
    }



    void AssignCopyTargets(List<GameObject> desks)
    {

        List<GameObject> shuffled = new List<GameObject>(desks);

        int nbCopy = gm.nbCopyNeeded;

        // Shuffle
        for (int i = 0; i < shuffled.Count; i++)
        {
            int rnd = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[rnd]) = (shuffled[rnd], shuffled[i]);
        }

        for (int i = 0; i < nbCopy; i++)
        {
            GameObject desk = shuffled[i];

            // Add trigger collider if missing
            if (!desk.TryGetComponent<Collider>(out Collider col))
            {
                BoxCollider box = desk.AddComponent<BoxCollider>();
                box.isTrigger = true;
            }
            else
            {
                col.isTrigger = true;
            }

            // Add copy target script
            desk.AddComponent<CopyTargetDesk>();

            //Glow effect
            MakeDeskGlow(desk, Color.yellow);
        }
        for(int i = nbCopy; i < desks.Count; i++)
        {
            GameObject desk = shuffled[i];

            if (desk.TryGetComponent<Collider>(out Collider col))
            {

            }
            else
            {
                
            }
        }
    }

    void MakeDeskGlow(GameObject desk,Color c)
    {
        Renderer[] renderers = desk.GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", c * 0.5f);
            }
        }
    }

    void GenerateRandomObstacles()
    {
        if (obstaclePrefabs.Count == 0 || obstacleCount <= 0)
            return;

        float roomWidth = width * (segmentLength + pillarSize);
        float roomLength = length * (segmentLength + pillarSize);

        List<WallSide> validWalls = new List<WallSide>
    {
        WallSide.North,
        WallSide.South,
        WallSide.East,
        WallSide.West
    };

        // Remove door wall
        validWalls.Remove(doorWall);

        // Remove outside walls (they may have windows)
        foreach (WallSide wall in outsideWalls)
            validWalls.Remove(wall);

        if (validWalls.Count == 0)
            return;

        for (int i = 0; i < obstacleCount; i++)
        {
            int attempts = 0;
            bool placed = false;

            while (attempts < 10 && !placed)
            {
                attempts++;

                WallSide wall = validWalls[Random.Range(0, validWalls.Count)];
                GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];

                Vector3 pos = Vector3.zero;
                Quaternion rot = Quaternion.identity;

                switch (wall)
                {
                    case WallSide.North:
                        pos = new Vector3(
                            Random.Range(pillarSize, roomWidth - pillarSize),
                            0,
                            roomLength - obstacleWallOffset
                        );
                        rot = Quaternion.Euler(0, 180, 0);
                        break;

                    case WallSide.South:
                        pos = new Vector3(
                            Random.Range(pillarSize, roomWidth - pillarSize),
                            0,
                            obstacleWallOffset
                        );
                        rot = Quaternion.identity;
                        break;

                    case WallSide.East:
                        pos = new Vector3(
                            roomWidth - obstacleWallOffset,
                            0,
                            Random.Range(pillarSize, roomLength - pillarSize)
                        );
                        rot = Quaternion.Euler(0, -90, 0);
                        break;

                    case WallSide.West:
                        pos = new Vector3(
                            obstacleWallOffset,
                            0,
                            Random.Range(pillarSize, roomLength - pillarSize)
                        );
                        rot = Quaternion.Euler(0, 90, 0);
                        break;
                }

                if (!IsPositionFree(pos, prefab))
                    continue;

                GameObject obstacle = Instantiate(prefab, pos, rot, PropsParent);
                spawnedObjects.Add(obstacle);
                placed = true;
            }
        }


    }
    bool IsPositionFree(Vector3 position, GameObject prefab)
    {
        Collider col = prefab.GetComponentInChildren<Collider>();
        if (col == null)
            return true; // pas de collider donc on accepte

        Vector3 size = col.bounds.size;
        Vector3 halfExtents = size * 0.5f;

        // Décalage léger en hauteur pour éviter le sol
        Vector3 checkCenter = position + Vector3.up * halfExtents.y;

        Collider[] hits = Physics.OverlapBox(
            checkCenter,
            halfExtents,
            Quaternion.identity,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (hit.transform.IsChildOf(PropsParent))
            {
                if (!hit.isTrigger)
                    return false;
            }
        }

        return true;
    }

    Vector3[] GenerateTeacherPatrolPoints()
    {
        float walloffset = 1.0f;
        float segmentLength = walllength.z;
        float pillarSize = pillarlength.z;

        float roomWidth = width * (segmentLength + pillarSize);
        float roomLength = length * (segmentLength + pillarSize);

        Vector3[] positions =
        {
            new Vector3(walloffset, 0, walloffset),
            new Vector3(walloffset, 0, roomLength - walloffset),
            new Vector3(roomWidth - walloffset, 0, roomLength - walloffset),
            new Vector3(roomWidth - walloffset, 0, walloffset)
        };

        foreach (Vector3 position in positions)
        {
            Instantiate(
                patrolPointsPrefab,
                position,
                Quaternion.identity
            );
        }

        return positions;
    }
    void GenerateTeacher()
    {

        float xPos = segmentLength;
        float zPos = segmentLength / 2;

        Vector3 teacherPos = new Vector3(xPos, 0, zPos);

        teacherCharacter = Instantiate(
            teacherCharacter,
            teacherPos + new Vector3(2, 0, -1f),
            Quaternion.Euler(0, 180, 0)
        );
        teacherCharacter.transform.localScale = 2.0f * Vector3.one;

        ProfessorAI ai = teacherCharacter.GetComponent<ProfessorAI>();
        ai.patrolPoints = GenerateTeacherPatrolPoints();
    }


    void GenerateCameras()
    {
        if (Cameras == null)
            Cameras = new List<Camera>();
        else
            Cameras.Clear();

        float roomWidth = width * (segmentLength + pillarSize);
        float roomLength = length * (segmentLength + pillarSize);

        Vector3 roomCenter = new Vector3(
            roomWidth / 2f,
            0,
            roomLength / 2f
        );

        Vector3[] cameraPositions =
        {
        new Vector3(cameraOffset, pillarlength.y, cameraOffset), // Bottom-left
        new Vector3(roomWidth - cameraOffset, pillarlength.y, cameraOffset), // Bottom-right
        new Vector3(roomWidth - cameraOffset, pillarlength.y, roomLength - cameraOffset), // Top-right
        new Vector3(cameraOffset, pillarlength.y, roomLength - cameraOffset) // Top-left
        };

        for (int i = 0; i < cameraPositions.Length; i++)
        {
            Camera cam = Instantiate(
                cameraPrefab,
                cameraPositions[i],
                Quaternion.identity,
                PropsParent
            );

            cam.transform.LookAt(roomCenter);
            cam.gameObject.SetActive(i == 0); // Only first active

            Cameras.Add(cam);
        }
    }

    void GeneratePlayer()
    {
        Player = Instantiate(Player, playerDesk.transform.position + Vector3.one, Quaternion.identity);
        Player.transform.localScale = 2.0f * Vector3.one;
        Player.GetComponent<PlayerControls>().Cameras = Cameras;
        teacherCharacter.GetComponent<ProfessorAI>().player = Player;
    }
}
