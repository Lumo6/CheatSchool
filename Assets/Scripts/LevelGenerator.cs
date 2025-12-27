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
    public float spacing = 1.5f;

    //taille de la salle en nombre de préfab de murs
    private int length = 3;
    private int width = 3;

    [Header("Navigation")]
    public NavMeshSurface navMeshSurface;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    private Vector3 playerStartPos;
    private Vector3 copyTargetPos;

    private Vector3 walllength;
    private Vector3 pillarlength;

    public GameObject Player;

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

        Player.transform.position = playerStartPos;
        Player.transform.rotation = Quaternion.identity;
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
        // Position du joueur
        playerStartPos = new Vector3(Random.Range(1, rows - 1), Player.transform.GetComponent<Renderer>().bounds.max.y ,Random.Range(1, columns - 1));
        
        // Table de la copie à copier
        do
        {
            copyTargetPos = new Vector3(Random.Range(1, rows - 1), 0 , Random.Range(1, columns - 1));
        } while (copyTargetPos == playerStartPos);
    }

    void GenerateFloor()
    {
        float segmentLength = walllength.z;
        float pillarSize = pillarlength.z;

        Vector3 pos = new Vector3((segmentLength * width + pillarSize * (width+1))/2, 0, (segmentLength * length + pillarSize * (length + 1))/2);
        GameObject floor = Instantiate(floorPrefab, pos, Quaternion.identity, PropsParent);
        int scale = Mathf.Max(width, length)+1;
        floor.transform.localScale = new Vector3(scale,0,scale);
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
        float deskHeight = deskBounds.size.y;



        float roomWidth = walllength.z * width + pillarlength.z * (width - 1);
        float spacingOffset = spacing;

        float gridWidth = columns * deskWidth + (columns - 1) * spacingOffset;

        
        float startX = (roomWidth - gridWidth) / 2f + deskWidth / 2f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                float xPos = startX + col * (deskWidth + spacingOffset);
                float zPos = row * spacingOffset + walllength.z;

                Vector3 position = new Vector3(
                    xPos,
                    0,
                    zPos
                );

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


}
