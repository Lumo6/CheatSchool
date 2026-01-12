using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;

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
    public GameObject windowWallPrefab;
    public GameObject doorPrefab;


    [Header("Room Size")]
    public int rows = 5;
    public int columns = 6;

    [Header("Student Desk Spacing")]
    public float rowSpacing = 1.5f;
    public float columnSpacing = 1.5f;


    [Header("Room Dimensions")]
    public int length = 3;
    public int width = 3;

    [Header("Navigation")]
    public NavMeshSurface navMeshSurface;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    [Header("Important Positions")]
    private Vector3 playerStartPos;
    [Header("Copy Targets")]
    public int copyTargetCount = 3;
    private List<GameObject> copyTargetPositions = new List<GameObject>();


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

    enum WallSide { North, South, East, West }
    private HashSet<WallSide> outsideWalls = new HashSet<WallSide>();
    private WallSide doorWall;

    public enum Difficulty { Easy, Medium, Hard }

    [Header("Difficulty")]
    public Difficulty difficulty = Difficulty.Easy;


    private void Start()
    {
        walllength = wallPrefab.GetComponent<MeshRenderer>().bounds.size;
        pillarlength = pillarPrefab.GetComponent<MeshRenderer>().bounds.size;
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        ClearLevel();

        // 0. Choisir une position dans la grille 3x3
        PickGridPosition();

        // Définir les positions importantes
        DefineKeyPositions();

        // 1. Générer le sol
        GenerateFloor();

        // 2. Générer les murs extérieurs avec piliers
        GenerateWallsWithPillars();

        // 3. Placer le bureau du professeur
        PlaceTeacherDesk();

        // 4. Générer les bureaux d'étudiants en rangées
        GenerateStudentDesks();

        // 5. Générer le NavMesh
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh généré avec succès!");
        }

        Player = Instantiate(
            Player, 
            playerStartPos, 
            Quaternion.identity
        );
        Player.transform.localScale = 2.0f * Vector3.one;

        GenerateCameras();

        Player.GetComponent<PlayerControls>().Cameras = Cameras;
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


    void DefineKeyPositions()
    {
        copyTargetPositions.Clear();

        // Player position
        playerStartPos = new Vector3(
            Random.Range(1, rows - 1),
            0,
            Random.Range(1, columns - 1)
        );
    }


    void GenerateFloor()
    {
        float segmentLength = walllength.z;
        float pillarSize = pillarlength.z;
        int scale = Mathf.Max(width, length) + 1;

        Vector3 pos = new Vector3((
            segmentLength * width + pillarSize * (width + 1)) / 2, 
            0, 
            (segmentLength * length + pillarSize * (length + 1)) / 2
        );
        GameObject floor = Instantiate(
            floorPrefab, 
            pos, 
            Quaternion.identity, 
            PropsParent
        );
        floor.transform.localScale = new Vector3(scale, 0, scale);
        spawnedObjects.Add(floor);

        pos = new Vector3((
            segmentLength * width + pillarSize * (width + 1)) / 2, 
            pillarlength.y, 
            (segmentLength * length + pillarSize * (length + 1)) / 2
        );
        GameObject roof = Instantiate(
            floorPrefab, 
            pos, 
            Quaternion.identity, 
            PropsParent
        );
        roof.transform.localScale = new Vector3(scale, 0, scale);
        spawnedObjects.Add(roof);
    }

    void GenerateWallsWithPillars()
    {
        // Compute which walls are outside + where the door goes
        ComputeWallLogic();

        float segmentLength = walllength.z;
        float pillarSize = pillarlength.z;

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
                        windowWallPrefab, 
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
                        windowWallPrefab, 
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
                        windowWallPrefab, 
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
                        windowWallPrefab, 
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
        float segmentLength = walllength.z;

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

        // Teacher
        teacherCharacter = Instantiate(
            teacherCharacter,
            teacherPos + new Vector3(0, 0, -1f),
            Quaternion.Euler(0, 180, 0)
        );
        teacherCharacter.transform.localScale = 2.0f * Vector3.one;

        // === PATROL POINTS ===
        ProfessorAI ai = teacherCharacter.GetComponent<ProfessorAI>();

        if (ai != null)
        {
            List<Transform> patrols = GenerateProfessorPatrolPoints(teacherPos);
            ai.SetPatrolPoints(patrols);
            ai.SetDifficulty(difficulty);

        }
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
                allDesks.Add(desk);

                // Place random character at desk
                GameObject characterPrefab = rdmcharacters[Random.Range(0, rdmcharacters.Count)];
                GameObject character = Instantiate(
                    characterPrefab,
                    position + new Vector3(0, 0, -0.5f),
                    Quaternion.Euler(0, 180, 0)
                );
                character.transform.localScale = 2.0f * Vector3.one;
            }
        }

        AssignCopyTargets(allDesks);
    }


    void AssignCopyTargets(List<GameObject> desks)
    {
        // Safety
        copyTargetCount = Mathf.Min(copyTargetCount, desks.Count);

        List<GameObject> shuffled = new List<GameObject>(desks);

        // Shuffle
        for (int i = 0; i < shuffled.Count; i++)
        {
            int rnd = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[rnd]) = (shuffled[rnd], shuffled[i]);
        }

        for (int i = 0; i < copyTargetCount; i++)
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
        }
    }

    void GenerateCameras()
    {
        if(Cameras == null)
            Cameras = new List<Camera>();
        else
            Cameras.Clear();

        float segmentLength = walllength.z;
        float pillarSize = pillarlength.z;

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
                Quaternion.identity
            );

            cam.transform.LookAt(roomCenter);
            cam.gameObject.SetActive(i == 0); // Only first active

            Cameras.Add(cam);
        }
    }

    List<Transform> GenerateProfessorPatrolPoints(Vector3 deskPosition)
    {
        List<Transform> patrolPoints = new List<Transform>();

        float segmentLength = walllength.z;
        float pillarSize = pillarlength.z;

        float roomWidth = width * (segmentLength + pillarSize);
        float roomLength = length * (segmentLength + pillarSize);

        float wallOffset = 1.2f; // Prevent walking into walls

        // --- Desk patrol point (always exists) ---
        patrolPoints.Add(CreatePatrolPoint(deskPosition, "DeskPoint"));

        if (difficulty == Difficulty.Easy)
            return patrolPoints;

        // --- Room corners ---
        Vector3[] corners =
        {
        new Vector3(wallOffset, 0, wallOffset),
        new Vector3(roomWidth - wallOffset, 0, wallOffset),
        new Vector3(roomWidth - wallOffset, 0, roomLength - wallOffset),
        new Vector3(wallOffset, 0, roomLength - wallOffset)
    };

        if (difficulty == Difficulty.Medium)
        {
            patrolPoints.Add(CreatePatrolPoint(corners[0], "CornerA"));
            patrolPoints.Add(CreatePatrolPoint(corners[2], "CornerB"));
        }
        else if (difficulty == Difficulty.Hard)
        {
            for (int i = 0; i < corners.Length; i++)
            {
                patrolPoints.Add(CreatePatrolPoint(corners[i], $"Corner{i}"));
            }
        }

        return patrolPoints;
    }
    Transform CreatePatrolPoint(Vector3 position, string name)
    {
        GameObject point = new GameObject(name);
        point.transform.position = position;
        return point.transform;
    }

}
