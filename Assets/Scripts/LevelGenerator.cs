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
    public GameObject doorPrefab;
    public GameObject pc_etuPrefab;
    public GameObject pc_profPrefab;


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
    private List<Vector3> copyTargetPositions = new List<Vector3>();


    [Header("Lengths")]
    private Vector3 walllength;
    private Vector3 pillarlength;

    [Header("Cameras")]
    private List<Camera> Cameras;

    [Header("Player")]
    public GameObject Player;
    

    [Header("Camera")]
    public Camera cameraPrefab;
    public float cameraHeight = 5f;
    public float cameraOffset = 1f;

    private void Start()
    {
        walllength = wallPrefab.GetComponent<MeshRenderer>().bounds.size;
        pillarlength = pillarPrefab.GetComponent<MeshRenderer>().bounds.size;
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        ClearLevel();

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

        Player = Instantiate(Player, playerStartPos, Quaternion.identity);
        
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

    void DefineKeyPositions()
    {
        copyTargetPositions.Clear();

        // Player position
        playerStartPos = new Vector3(
            Random.Range(1, rows - 1),
            Player.GetComponent<Renderer>().bounds.max.y,
            Random.Range(1, columns - 1)
        );

        // Generate multiple copy target positions
        int attempts = 0;
        while (copyTargetPositions.Count < copyTargetCount && attempts < 100)
        {
            Vector3 pos = new Vector3(
                Random.Range(1, rows - 1),
                0,
                Random.Range(1, columns - 1)
            );

            // Avoid player position and duplicates
            if (pos == playerStartPos || copyTargetPositions.Contains(pos))
            {
                attempts++;
                continue;
            }

            copyTargetPositions.Add(pos);
        }
    }


    void GenerateFloor()
    {
        float segmentLength = walllength.z;
        float pillarSize = pillarlength.z;

        Vector3 pos = new Vector3((segmentLength * width + pillarSize * (width + 1)) / 2, 0, (segmentLength * length + pillarSize * (length + 1)) / 2);
        GameObject floor = Instantiate(floorPrefab, pos, Quaternion.identity, PropsParent);
        int scale = Mathf.Max(width, length) + 1;
        floor.transform.localScale = new Vector3(scale, 0, scale);
        spawnedObjects.Add(floor);
    }

    void GenerateWallsWithPillars()
    {
        // Calculate wall segment length and pillar positions
        float segmentLength = walllength.z;
        float pillarSize = pillarlength.z;

        // Generate walls along each side of the room

        // 1. NORTH WALL (top, along width)
        for (int i = 0; i <= width; i++)
        {
            // Place pillar
            float xPos = i * (segmentLength + pillarSize);
            float zPos = length * (segmentLength + pillarSize);
            Vector3 pillarPos = new Vector3(xPos, pillarlength.y / 2, zPos);
            GameObject pillar = Instantiate(pillarPrefab, pillarPos, Quaternion.identity, PropsParent);
            spawnedObjects.Add(pillar);

            // Place wall segment (except after last pillar)
            if (i < width)
            {
                float wallXPos = xPos + pillarSize / 2 + segmentLength / 2;
                Vector3 wallPos = new Vector3(wallXPos, walllength.y / 2, zPos);
                GameObject wall = Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0), PropsParent);
                spawnedObjects.Add(wall);
            }
        }

        // 2. SOUTH WALL (bottom, along width)
        for (int i = 0; i <= width; i++)
        {
            // Place pillar
            float xPos = i * (segmentLength + pillarSize);
            float zPos = 0;
            Vector3 pillarPos = new Vector3(xPos, pillarlength.y / 2, zPos);
            GameObject pillar = Instantiate(pillarPrefab, pillarPos, Quaternion.identity, PropsParent);
            spawnedObjects.Add(pillar);

            // Place wall segment (except after last pillar)
            if (i < width)
            {
                float wallXPos = xPos + pillarSize / 2 + segmentLength / 2;
                Vector3 wallPos = new Vector3(wallXPos, walllength.y / 2, zPos);
                GameObject wall = Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0), PropsParent);
                spawnedObjects.Add(wall);
            }
        }

        // 3. WEST WALL (left, along length)
        for (int i = 0; i <= length; i++)
        {
            float xPos = 0;
            float zPos = i * (segmentLength + pillarSize);

            // Skip pillars at corners (already placed)
            if (i > 0 && i < length)
            {
                Vector3 pillarPos = new Vector3(xPos, pillarlength.y / 2, zPos);
                GameObject pillar = Instantiate(pillarPrefab, pillarPos, Quaternion.identity, PropsParent);
                spawnedObjects.Add(pillar);
            }

            // Place wall segment (except after last pillar)
            if (i < length)
            {
                float wallZPos = zPos + pillarSize / 2 + segmentLength / 2;
                Vector3 wallPos = new Vector3(xPos, walllength.y / 2, wallZPos);
                GameObject wall = Instantiate(wallPrefab, wallPos, Quaternion.identity, PropsParent);
                spawnedObjects.Add(wall);
            }
        }

        // 4. EAST WALL (right, along length)
        for (int i = 0; i <= length; i++)
        {
            float xPos = width * (segmentLength + pillarSize);
            float zPos = i * (segmentLength + pillarSize);

            // Skip pillars at corners (already placed)
            if (i > 0 && i < length)
            {
                Vector3 pillarPos = new Vector3(xPos, pillarlength.y / 2, zPos);
                GameObject pillar = Instantiate(pillarPrefab, pillarPos, Quaternion.identity, PropsParent);
                spawnedObjects.Add(pillar);
            }

            // Place wall segment (except after last pillar)
            if (i < length)
            {
                float wallZPos = zPos + pillarSize / 2 + segmentLength / 2;
                Vector3 wallPos = new Vector3(xPos, walllength.y / 2, wallZPos);
                GameObject wall = Instantiate(wallPrefab, wallPos, Quaternion.identity, PropsParent);
                spawnedObjects.Add(wall);
            }
        }
    }

    private void PlaceTeacherDesk()
    {
        float segmentLength = walllength.z;
        float pillarSize = pillarlength.z;


        float xPos = segmentLength;
        float zPos = segmentLength / 2;

        Vector3 teacherPos = new Vector3(xPos, 0, zPos);
        GameObject teacherDesk = Instantiate(pc_profPrefab, teacherPos, Quaternion.identity, PropsParent);
        spawnedObjects.Add(teacherDesk);
    }

    private void GenerateStudentDesks()
    {
        Renderer[] renderers = pc_etuPrefab.GetComponentsInChildren<Renderer>();

        Bounds deskBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            deskBounds.Encapsulate(renderers[i].bounds);
        }

        float deskWidth = deskBounds.size.x;
        float deskDepth = deskBounds.size.z;

        float roomWidth = walllength.z * width + pillarlength.z * (width - 1);

        // Total width of the desk grid (X axis)
        float gridWidth =
            columns * deskWidth +
            (columns - 1) * columnSpacing;

        // Center the grid in the room
        float startX = (roomWidth - gridWidth) / 2f + deskWidth / 2f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                float xPos = startX + col * (deskWidth + columnSpacing);
                float zPos = walllength.z + row * (deskDepth + rowSpacing);

                Vector3 position = new Vector3(xPos, 0, zPos);

                GameObject studentDesk = Instantiate(
                    pc_etuPrefab,
                    position,
                    Quaternion.identity,
                    PropsParent
                );

                spawnedObjects.Add(studentDesk);
            }
        }
    }


    void GenerateCameras()
    {
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
        new Vector3(cameraOffset, cameraHeight, cameraOffset), // Bottom-left
        new Vector3(roomWidth - cameraOffset, cameraHeight, cameraOffset), // Bottom-right
        new Vector3(cameraOffset, cameraHeight, roomLength - cameraOffset), // Top-left
        new Vector3(roomWidth - cameraOffset, cameraHeight, roomLength - cameraOffset) // Top-right
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



}
