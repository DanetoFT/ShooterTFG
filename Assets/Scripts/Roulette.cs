using System.Collections.Generic;
using UnityEngine;

public class Roulette : MonoBehaviour
{
    public static Roulette Instance { get; private set; }

    [Header("Spin")]
    public float RotatePower = 1080f;     // grados/segundo iniciales
    public float StopPower = 400f;        // frenado

    private float angularSpeed;
    private bool spinning;

    private float timer;

    public ParticleSystem particulasWin;

    public RouletteSector[] sectors;

    public float radius = 2f;
    public int smoothness = 10;

    public Transform wheel;

    Mesh mesh;
    MeshFilter meshFilter;

    public Transform pointer;

    private float currentRotation;

    private bool hasAmmo = false;

    private void Awake()
    {
        Instance = this;

        meshFilter = GetComponent<MeshFilter>();

        mesh = new Mesh();
        mesh.name = "Roulette";

        meshFilter.sharedMesh = mesh;

        foreach (var sector in sectors)
        {
            sector.currentAmmo = sector.maxAmmo;
        }

        GenerateMesh();
    }

    private void Update()
    {
        if (!spinning && hasAmmo)
            return;

        // Girar únicamente sobre el eje Z local
        currentRotation += angularSpeed * Time.deltaTime;

        transform.localRotation =
            Quaternion.Euler(90, -90, currentRotation);

        // Frenar
        angularSpeed -= StopPower * Time.deltaTime;

        if (angularSpeed <= 0)
        {
            angularSpeed = 0;

            timer += Time.deltaTime;

            if (timer >= 0.5f)
            {
                particulasWin.Play();
                spinning = false;
                timer = 0f;

                GetReward();
            }
        }
    }

    private void OnValidate()
    {
        //CalculateAngles();

        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Roulette";

            if (meshFilter != null)
                meshFilter.sharedMesh = mesh;
        }

        //GenerateMesh();
    }

    void GenerateMesh()
    {
        mesh.Clear();


        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Color> colors = new();

        float currentAngle = 0;


        foreach (var sector in sectors)
        {
            sector.startAngle = currentAngle;
            sector.endAngle = currentAngle + sector.angle;

            CreateSector(
                vertices,
                triangles,
                colors,
                currentAngle,
                sector.angle,
                sector.color);

            currentAngle += sector.angle;
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(colors);

        mesh.RecalculateNormals();
    }

    void CreateSector(
    List<Vector3> vertices,
    List<int> triangles,
    List<Color> colors,
    float start,
    float angle,
    Color color)
    {
        int center = vertices.Count;

        vertices.Add(Vector3.zero);
        colors.Add(color);

        int segments = Mathf.Max(2, Mathf.RoundToInt(smoothness * angle / 60f));

        for (int i = 0; i <= segments; i++)
        {
            float a = start + angle * i / segments;

            float rad = a * Mathf.Deg2Rad;

            Vector3 p = new Vector3(
                Mathf.Cos(rad),
                Mathf.Sin(rad),
                0) * radius;

            vertices.Add(p);
            colors.Add(color);
        }

        for (int i = 0; i < segments; i++)
        {
            triangles.Add(center);
            triangles.Add(center + i + 1);
            triangles.Add(center + i + 2);
        }
    }

    void CalculateAngles()
    {
        float totalWeight = 0;

        foreach (var sector in sectors)
        {
            totalWeight += sector.weight;
        }

        float currentAngle = 0;

        foreach (var sector in sectors)
        {
            sector.angle = (sector.weight / totalWeight) * 360f;

            sector.originalAngle = sector.angle;

            sector.startAngle = currentAngle;
            sector.endAngle = currentAngle + sector.angle;
            sector.middleAngle = sector.startAngle + sector.angle / 2f;

            currentAngle += sector.angle;
        }
    }

    private MaterialPropertyBlock block;

    /*private void ApplyColors()
    {
        if (block == null)
            block = new MaterialPropertyBlock();

        foreach (var sector in sectors)
        {
            if (sector == null || sector.meshRenderer == null)
                continue;

            sector.meshRenderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", sector.color);
            sector.meshRenderer.SetPropertyBlock(block);
        }
    }*/

    public void Rotete()
    {
        if (spinning)
            return;

        angularSpeed = RotatePower;
        spinning = true;
    }

    private void GetReward()
    {
        Vector3 localPoint = transform.InverseTransformPoint(pointer.position);

        float angle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;

        if (angle < 0)
            angle += 360;


        foreach (var sector in sectors)
        {
            if (angle >= sector.startAngle &&
               angle < sector.endAngle)
            {
                Win(sector);
                return;
            }
        }
    }

    private RouletteSector currentWeaponSector;

    private void Win(RouletteSector sector)
    {
        Debug.Log("GANADOR: " + sector.weaponName);

        // Desactivar todas las armas
        foreach (var s in sectors)
        {
            if (s.weaponObject != null)
                s.weaponObject.SetActive(false);
        }

        // Activar arma ganadora
        if (sector.weaponObject != null)
            sector.weaponObject.SetActive(true);

        currentWeaponSector = sector;

        // Mostrar indicador de munición
        UpdateAmmoIndicator();
    }

    public void UseAmmo(int amount = 1)
    {
        if (currentWeaponSector == null)
            return;

        currentWeaponSector.currentAmmo -= amount;

        currentWeaponSector.currentAmmo =
            Mathf.Max(0, currentWeaponSector.currentAmmo);

        UpdateAmmoIndicator();

        if (currentWeaponSector.currentAmmo <= 0)
        {
            LoseWeapon();
        }
    }

    private void LoseWeapon()
    {
        if (currentWeaponSector.weaponObject != null)
            currentWeaponSector.weaponObject.SetActive(false);

        currentWeaponSector = null;

        CalculateAngles();
        GenerateMesh();
    }

    private void UpdateAmmoIndicator()
    {
        if (currentWeaponSector == null)
            return;

        if (currentWeaponSector.maxAmmo <= 0)
            return;

        float ammoPercentage =
            (float)currentWeaponSector.currentAmmo /
            currentWeaponSector.maxAmmo;

        float ammoAngle = ammoPercentage * 360f;

        GenerateAmmoMesh(
            ammoAngle,
            currentWeaponSector.color
        );
    }

    private void GenerateAmmoMesh(float angle, Color color)
    {
        mesh.Clear();

        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Color> colors = new();

        if (angle <= 0f)
            return;

        float startAngle = 0f;

        CreateSector(
            vertices,
            triangles,
            colors,
            startAngle,
            angle,
            color
        );

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(colors);

        mesh.RecalculateNormals();
    }
}