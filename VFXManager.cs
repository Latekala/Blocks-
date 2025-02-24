using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [Header("Particle Systems")]
    public ParticleSystem lineClearParticles;
    public ParticleSystem blockPlaceParticles;
    public ParticleSystem gameOverParticles;
    
    [Header("Line Clear Effect Settings")]
    public Color lineClearColor = Color.cyan;
    public float lineClearDuration = 0.5f;
    public int particlesPerCell = 10;
    
    [Header("Block Place Effect Settings")]
    public Color blockPlaceColor = Color.white;
    public float blockPlaceDuration = 0.3f;
    
    [Header("Game Over Effect Settings")]
    public Color gameOverColor = Color.red;
    public float gameOverDuration = 1.5f;

    [Header("Block Selection Effects")]
    public Color selectionGlowColor = new Color(1f, 1f, 1f, 0.5f);
    public float glowPulseSpeed = 2f;
    public float glowIntensity = 1.2f;

    private void Start()
    {
        InitializeParticleSystems();
    }

    private void InitializeParticleSystems()
    {
        // Initialize Line Clear Effect
        if (lineClearParticles == null)
        {
            GameObject lineVFX = new GameObject("LineClearVFX");
            lineVFX.transform.parent = transform;
            lineClearParticles = lineVFX.AddComponent<ParticleSystem>();
            lineClearParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            SetupLineClearParticles();
        }

        // Initialize Block Place Effect
        if (blockPlaceParticles == null)
        {
            GameObject blockVFX = new GameObject("BlockPlaceVFX");
            blockVFX.transform.parent = transform;
            blockPlaceParticles = blockVFX.AddComponent<ParticleSystem>();
            blockPlaceParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            SetupBlockPlaceParticles();
        }

        // Initialize Game Over Effect
        if (gameOverParticles == null)
        {
            GameObject gameOverVFX = new GameObject("GameOverVFX");
            gameOverVFX.transform.parent = transform;
            gameOverParticles = gameOverVFX.AddComponent<ParticleSystem>();
            gameOverParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            SetupGameOverParticles();
        }
    }

    private void SetupLineClearParticles()
    {
        var main = lineClearParticles.main;
        main.playOnAwake = false;
        main.duration = lineClearDuration;
        main.loop = false;
        main.startLifetime = lineClearDuration;
        main.startSpeed = 2f;
        main.startSize = 0.2f;
        main.startColor = lineClearColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = lineClearParticles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { 
            new ParticleSystem.Burst(0f, particlesPerCell) 
        });

        var shape = lineClearParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = Vector3.one;
    }

    private void SetupBlockPlaceParticles()
    {
        var main = blockPlaceParticles.main;
        main.playOnAwake = false;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.2f;
        main.startColor = blockPlaceColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = blockPlaceParticles.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { 
            new ParticleSystem.Burst(0f, 30) 
        });

        var rotationOverLifetime = blockPlaceParticles.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-180f, 180f);
    }

    private void SetupGameOverParticles()
    {
        var main = gameOverParticles.main;
        main.playOnAwake = false;
        main.duration = gameOverDuration;
        main.loop = false;
        main.startLifetime = gameOverDuration;
        main.startSpeed = 3f;
        main.startSize = 0.3f;
        main.startColor = gameOverColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = gameOverParticles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { 
            new ParticleSystem.Burst(0f, 100) 
        });

        var shape = gameOverParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(10f, 10f, 1f);
    }

    public void PlayLineClearEffect(Vector3 position, bool isRow, float length)
    {
        lineClearParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var shape = lineClearParticles.shape;
        if (isRow)
        {
            shape.scale = new Vector3(length, 0.1f, 1f);
        }
        else
        {
            shape.scale = new Vector3(0.1f, length, 1f);
        }

        lineClearParticles.transform.position = position;
        lineClearParticles.Play();
    }

    public void PlayBlockPlaceEffect(Vector3 position)
    {
        blockPlaceParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        blockPlaceParticles.transform.position = position;
        blockPlaceParticles.Play();
    }

    public void PlayGameOverEffect(Vector3 centerPosition)
    {
        gameOverParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        gameOverParticles.transform.position = centerPosition;
        gameOverParticles.Play();
    }
}