using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine.UIElements;

/// <summary>
/// Génère dynamiquement les niveaux de jeu, y compris le sol, les murs, les bureaux, les obstacles,
/// les caméras de surveillance et les personnages (joueur et professeur).
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    [Header("Parent of Props")]
    [SerializeField] private Transform PropsParent; // Parent de tous les objets générés pour garder la hiérarchie propre

    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject wallDoorPrefab;
    [SerializeField] private GameObject pillarPrefab;
    [SerializeField] private GameObject pc_etuPrefab;  // Bureau étudiant
    [SerializeField] private GameObject pc_profPrefab; // Bureau professeur
    [SerializeField] private List<GameObject> windowWallPrefab; // Mur avec fenêtre
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private GameObject patrolPointsPrefab; // Points de patrouille pour le professeur

    [Header("Room Size")]
    [SerializeField] private int rows = 5;    // Nombre de rangées de bureaux étudiants
    [SerializeField] private int columns = 6; // Nombre de colonnes de bureaux étudiants

    [Header("Student Desk Spacing")]
    [SerializeField] private float rowSpacing = 1.5f;    // Espacement entre les rangées de bureaux
    [SerializeField] private float columnSpacing = 1.5f; // Espacement entre les colonnes de bureaux

    [Header("Room Dimensions")]
    [SerializeField] private int length = 3; // Nombre de segments de longueur du mur
    [SerializeField] private int width = 3;  // Nombre de segments de largeur du mur

    [Header("Player Desk")]
    private GameObject playerDesk; // Référence au bureau du joueur
    [SerializeField] private Vector3 playerDeskOffset = new Vector3(0, 0, 1f); // Décalage de position pour le joueur

    [Header("Lengths")]
    private Vector3 walllength;   // Taille d'un segment de mur
    private Vector3 pillarlength; // Taille d'un pilier

    [Header("Cameras")]
    private List<Camera> Cameras;           // Liste des caméras de surveillance
    [SerializeField] private Camera cameraPrefab;
    [SerializeField] private float cameraOffset = 1f; // Décalage de position des caméras

    [Header("World Grid")]
    private Vector2Int gridPos; // Position de la salle dans la grille globale (0..2, 0..2)

    [Header("Characters")]
    [SerializeField] private List<GameObject> rdmcharacters = new List<GameObject>(); // Liste de PNJ étudiants
    [SerializeField] private List<GameObject> copycharacters = new List<GameObject>(); // Non utilisé ici mais réservé pour futur
    [SerializeField] private GameObject teacherCharacter; // Prefab du professeur
    [SerializeField] private GameObject Player;           // Prefab du joueur

    [Header("Random Obstacles")]
    [SerializeField] private List<GameObject> obstaclePrefabs = new List<GameObject>();
    [SerializeField] private int obstacleCount = 5;          // Nombre d'obstacles à générer

    // Liste de tous les objets générés pour pouvoir les nettoyer facilement
    private List<GameObject> spawnedObjects = new List<GameObject>();

    // Enumération des côtés de murs pour la logique de génération
    enum WallSide { North, South, East, West }
    private HashSet<WallSide> outsideWalls = new HashSet<WallSide>(); // Murs exposés à l'extérieur
    private WallSide doorWall;  // Mur sur lequel sera la porte
    private float segmentLength; // Longueur d'un segment de mur
    private float pillarSize;    // Taille d'un pilier
    private GameObject floor;    // Référence au sol généré
    private NavMeshSurface navMeshSurface; // Surface de navigation pour IA
    private List<GameObject> copyTargetPositions = new List<GameObject>(); // Positions des bureaux à copier
    private GameManager gm; // Référence au GameManager
    private DifficultySettings ds; // Référence aux paramètres de difficulté actuels

    /// <summary>
    /// Génère le niveau complet : sol, murs, bureaux, obstacles, professeur, caméras et joueur.
    /// </summary>
    void GenerateLevel()
    {
        // Clear previous level
        ClearLevel();

        // Get GameManager instance
        gm = GameManager.Instance;

        // Pull values from GameManager difficulty settings
        ds = gm.currentDifficultySettings;
        width = ds.width;
        length = ds.length;
        rows = ds.rows;
        columns = ds.columns;
        obstacleCount = ds.obstacleCount;

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

    /// <summary>
    /// Initialisation au démarrage du jeu.
    /// </summary>
    private void Start()
    {
        // Get lengths
        walllength = wallPrefab.GetComponent<MeshRenderer>().bounds.size;
        pillarlength = pillarPrefab.GetComponent<MeshRenderer>().bounds.size;
        segmentLength = walllength.z;
        pillarSize = pillarlength.z;

        GenerateLevel();
    }

    /// <summary>
    /// Nettoie tous les objets générés du niveau précédent.
    /// </summary>
    void ClearLevel()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();
    }

    /// <summary>
    /// Sélectionne une position aléatoire dans la grille 3x3 pour la salle actuelle.
    /// </summary>
    void PickGridPosition()
    {
        gridPos = new Vector2Int(
            Random.Range(0, 3),
            Random.Range(0, 3)
        );

        Debug.Log($"Room grid position: {gridPos}");
    }

    /// <summary>
    /// Génère le sol et le plafond de la salle.
    /// </summary>
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

    /// <summary>
    /// Génère les murs de la salle avec des piliers, des fenêtres et une porte.
    /// </summary>
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

    /// <summary>
    /// Calcule quels murs sont exposés à l'extérieur et où placer la porte.
    /// </summary>
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

    /// <summary>
    /// Place le bureau du professeur dans la salle.
    /// </summary>
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

    /// <summary>
    /// Génère les bureaux des étudiants dans la salle.
    /// </summary>
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

    /// <summary>
    /// Assigne aléatoirement des bureaux comme cibles de copie.
    /// </summary>
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

    /// <summary>
    /// Applique un effet de lueur sur un bureau donné pour rendre le jeu plus facile en mode editor.
    /// </summary>
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

    /// <summary>
    /// Génère des obstacles aléatoires dans la salle en évitant les murs avec portes et fenêtres.
    /// </summary>
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

                float obstacleWallOffset = prefab.GetComponent<BoxCollider>().size.z / 2 + 0.1f;

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

    /// <summary>
    /// Vérifie si une position est libre pour placer un obstacle.
    /// </summary>
    bool IsPositionFree(Vector3 position, GameObject prefab)
    {
        BoxCollider col = prefab.GetComponentInChildren<BoxCollider>();
        if (col == null)
            return true; // pas de box collider donc on accepte

        Vector3 size = col.size;
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

    /// <summary>
    /// Génère les points de patrouille pour le professeur.
    /// </summary>
    Vector3[] GenerateTeacherPatrolPoints(int randomPointsCount = 3)
    {
        float wallOffset = 1.0f;
        float segmentLength = walllength.z;
        float pillarSize = pillarlength.z;

        float roomWidth = width * (segmentLength + pillarSize);
        float roomLength = length * (segmentLength + pillarSize);

        List<Vector3> positions = new List<Vector3>();

        //Les coins
        positions.Add(new Vector3(wallOffset, 0, wallOffset));
        positions.Add(new Vector3(wallOffset, 0, roomLength - wallOffset));
        positions.Add(new Vector3(roomWidth - wallOffset, 0, roomLength - wallOffset));
        positions.Add(new Vector3(roomWidth - wallOffset, 0, wallOffset));

        //Le centre de la salle
        Vector3 center = new Vector3(roomWidth / 2, 0, roomLength / 2);
        positions.Add(center);

        //Centre de chaque mur
        positions.Add(new Vector3(roomWidth / 2, 0, wallOffset));              // mur bas
        positions.Add(new Vector3(roomWidth / 2, 0, roomLength - wallOffset)); // mur haut
        positions.Add(new Vector3(wallOffset, 0, roomLength / 2));             // mur gauche
        positions.Add(new Vector3(roomWidth - wallOffset, 0, roomLength / 2)); // mur droit

        //Points aléatoires vers les bureaux
        for (int i = 0; i < randomPointsCount; i++)
        {
            float x = Random.Range(roomWidth * 0.25f, roomWidth * 0.75f);
            float z = Random.Range(roomLength * 0.25f, roomLength * 0.75f);
            positions.Add(new Vector3(x, 0, z));
        }

        // Instantiation des points
        foreach (Vector3 position in positions)
        {
            Instantiate(patrolPointsPrefab, position, Quaternion.identity);
        }

        return positions.ToArray();
    }

    /// <summary>
    /// Génère le personnage du professeur et lui assigne ses points de patrouille.
    /// </summary>
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
        ai.patrolPoints = GenerateTeacherPatrolPoints(ds.nbCopyNeeded);
    }

    /// <summary>
    /// Génère les caméras de surveillance dans les coins de la salle.
    /// </summary>
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

    /// <summary>
    /// Génère le personnage du joueur et l'assigne au bureau du joueur.
    /// </summary>
    void GeneratePlayer()
    {
        Player = Instantiate(Player, playerDesk.transform.position + Vector3.one, Quaternion.identity);
        Player.transform.localScale = 2.0f * Vector3.one;
        Player.GetComponent<PlayerControls>().Cameras = Cameras;
        teacherCharacter.GetComponent<ProfessorAI>().player = Player;
    }
}
