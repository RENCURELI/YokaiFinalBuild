using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public static List<EnemyBehaviour> all = new List<EnemyBehaviour>();

    public AsyncProcessId mainProcess = new AsyncProcessId();

    Vector3 posWhenPerformedAttack;

    public bool active = true;

    private static AsyncProcessId globalProcess;

    private void Awake()
    {
        all.Add(this);
        if (globalProcess == null)
        {
            globalProcess = new AsyncProcessId();
            UpdateAll(globalProcess);
        }
    }

    private void Start()
    {
        active = true;
        if (!alwaysSeePlayer)
            gameObject.SetActive(true);
        mainProcess = mainProcess.GetNew();
        startPosition = transform.position;
        startRotation = transform.rotation;
        if (!alwaysSeePlayer)
        {
            State_Patrol(mainProcess);
        } else
        {
            Invoke("StartNemesis", 10);
        }
        
    }

    void StartNemesis()
    {
        State_PermanentAttack(mainProcess);
        SoundsLibrary.PlaySoundEffect("Nemesis 6");
    }

    private void OnDisable()
    {
        mainProcess.Cancel();
    }

    private void OnDestroy()
    {
        mainProcess.Cancel();
        //all.Remove(this);
        //globalProcess.Cancel();
    }

    private void Log(string message)
    {
        Debug.Log("[Enemy Behaviour] > " + message);
    }

    private struct EnemyInput
    {
        public Vector3 position;
        public static int size
        {
            get
            {
                return sizeof(float) * 3;
            }
        }
    }

    private struct EnemyOutput
    {
        public int flag;
        public static int size
        {
            get
            {
                return sizeof(int);
            }
        }
    }

    private async void UpdateAll(AsyncProcessId process)
    {
        const float deltaTime = 1.0f;

        await Task.Delay(System.TimeSpan.FromSeconds(2.0f));

        Player player = FindObjectOfType<Player>();

        ComputeShader behaviourShader = Resources.Load<ComputeShader>("BehaviourShader");

        int kernelId = behaviourShader.FindKernel("CSEnemyUpdate");
        ComputeBuffer inputBuffer = new ComputeBuffer(all.Count, EnemyInput.size);
        EnemyInput[] inputs = new EnemyInput[all.Count];
        for (int i = 0; i < all.Count; i++)
        {
            inputs[i] = new EnemyInput() { position = all[i].transform.position};
        }
        inputBuffer.SetData(inputs);
        behaviourShader.SetBuffer(kernelId, "enemyInputs", inputBuffer);
        behaviourShader.SetInt("enemyCount", all.Count);
        ComputeBuffer outputBuffer = new ComputeBuffer(all.Count, EnemyOutput.size);
        EnemyOutput[] outputs = new EnemyOutput[all.Count];
        for (int i = 0; i < all.Count; i++)
        {
            outputs[i] = new EnemyOutput() { flag = 0 };
        }
        outputBuffer.SetData(outputs);
        behaviourShader.SetBuffer(kernelId, "enemyOutputs", outputBuffer);

        int playerPropId = Shader.PropertyToID("playerPosition");
        int enemiesBufferPropId = Shader.PropertyToID("enemyInputs");

        while (process.valid)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
            if (process.Canceled) break;

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i] == null) continue;
                inputs[i] = new EnemyInput() { position = all[i].transform.position };
            }
            inputBuffer.SetData(inputs);
            behaviourShader.SetBuffer(kernelId, enemiesBufferPropId, inputBuffer);
            behaviourShader.SetVector(playerPropId, player.transform.position);
            behaviourShader.Dispatch(kernelId, all.Count, 1, 1);

            EnemyOutput[] dispatchedOutput = new EnemyOutput[all.Count];
            outputBuffer.GetData(dispatchedOutput);

            bool updatedSomething = false;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i] == null) continue;
                updatedSomething = true;
                if (dispatchedOutput[i].flag == 1)
                {
                    if (!all[i].active) all[i].ResetSingle();
                } else
                {
                    if (all[i].active) all[i].Stop();
                }
            }
            if (!updatedSomething) globalProcess.Cancel();
        }

        Debug.Log("Process cancelled");
        
        inputBuffer.Dispose();
        outputBuffer.Dispose();
    }

    #region GENERAL

    private async Task TurnToTarget(AsyncProcessId process, Vector3 target, float speed)
    {
        if (process.Canceled) return;
        Vector3 pos = transform.position;
        target.y = pos.y = 0;

        Vector3 toTarget = target - pos;
        Vector3 startDir = transform.forward;

        Quaternion beginRotation = Quaternion.LookRotation(startDir, Vector3.up);
        Quaternion endRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);

        float angles = Vector3.Angle(toTarget, startDir);
        //float turnSign = (Vector3.Angle(toTarget, transform.right) > 90) ? -1 : 1;

        float turnTime = angles / speed;
        float timer = turnTime;

        float deltaTime = 0.03f;

        while (timer > 0)
        {
            timer -= deltaTime;
            float t = 1 - timer / turnTime;
            Quaternion rotation = Quaternion.Slerp(beginRotation, endRotation, t);
            transform.rotation = rotation;
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
            if (process.Canceled) return;
        }

        transform.rotation = endRotation;
    }

    public void ForceNemesisMoveToTarget(Vector3 dest)
    {/*
        mainProcess = mainProcess.GetNew();
        UnityEngine.AI.NavMeshHit hit;
        UnityEngine.AI.NavMesh.FindClosestEdge(dest, out hit, UnityEngine.AI.NavMesh.AllAreas);
        MoveToTarget(mainProcess, hit.position, attackSpeed);*/
    }

    public void AwakeNemesis()
    {
        //mainProcess = mainProcess.GetNew();
        //State_PermanentAttack(mainProcess);
    }

    private async Task MoveToTarget(AsyncProcessId process, Vector3 target, float speed)
    {
        if (process.Canceled) return;

        Vector3 beginPosition = transform.position;
        Vector3 endPosition = target;

        float deltaTime = 0.02f;

        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
        if (!UnityEngine.AI.NavMesh.CalculatePath(beginPosition, endPosition, UnityEngine.AI.NavMesh.AllAreas, path))
        {
            Log("Could not calculate path to target.");
            await Task.Delay(System.TimeSpan.FromSeconds(0.5f));
            //mainProcess = mainProcess.GetNew();
            return;
        }
        if (!(path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete))
        {
            Log("Could not calculate path to target.");
            await Task.Delay(System.TimeSpan.FromSeconds(0.5f));
            //mainProcess = mainProcess.GetNew();
            return;
        }

        Vector3[] corners = path.corners;

        int pathId = 0;

        while (pathId < corners.Length && process.valid)
        {
            Vector3 pathStart = transform.position;
            Vector3 pathEnd = corners[pathId];
            pathStart.y = pathEnd.y = transform.position.y;
            Vector3 toTarget = pathEnd - pathStart;

            float duration = toTarget.magnitude / speed;
            float timer = duration;

            while (timer > 0 && process.valid)
            {
                timer -= deltaTime;
                float t = 1 - timer / duration;
                Vector3 pos = Vector3.Lerp(pathStart, pathEnd, t);
                transform.position = pos;
                await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
                if (process.Canceled) return;
                if (playerWasHit) { process.Cancel(); return; }
            }
            if (playerWasHit) { process.Cancel(); return; }
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));

            pathId++;
        }

        //transform.position = endPosition;
    }

    private void ResetSingle()
    {
        mainProcess.Cancel();
        mainProcess = mainProcess.GetNew();
        transform.position = startPosition;
        currentWaypointId = 0;
        alerted = false;
        //State_Patrol(mainProcess);
        Start();
    }

    private void Stop()
    {
        active = false;
        mainProcess.Cancel();
        gameObject.SetActive(false);
    }

    private static void ResetAll()
    {
        playerWasHit = false;
        foreach (var i in all)
        {
            if (i != null)
            {
                i.ResetSingle();
            }
        }
    }

    #endregion GENERAL

    #region PATROL

    [Header("Patrol")]
    public bool loopPatrol = true;
    public Transform[] waypoints;
    private int currentWaypointId = 0;
    private Vector3 startPosition;
    private Quaternion startRotation;
    public float patrolSpeed = 3; // meters per second
    public float patrolTurnSpeed = 50; // angles per second
    [System.NonSerialized]
    private int patrolDirection = 1;

    private async Task ReachWaypoint(AsyncProcessId process, Transform waypoint)
    {
        Vector3 waypointPos = waypoint.position;
        await TurnToTarget(process, waypointPos, patrolTurnSpeed);
        await MoveToTarget(process, waypointPos, patrolSpeed);
    }

    private async void State_Patrol(AsyncProcessId process)
    {
        process.Cancel();
        mainProcess = process = mainProcess.GetNew();
        TransitionToAttack(process);
        //Log("Patrol Start");
        if (waypoints.Length == 0) PatrolRoutineGuard(process);
        else PatrolRoutineWaypoints(process);
    }

    private async void PatrolRoutineWaypoints(AsyncProcessId process)
    {
        while (process.valid)
        {
            if (currentWaypointId < 0 || currentWaypointId >= waypoints.Length) currentWaypointId = 0;
            Transform waypoint = waypoints[currentWaypointId];
            await ReachWaypoint(process, waypoint);
            currentWaypointId += patrolDirection;
            if (loopPatrol && currentWaypointId >= waypoints.Length) currentWaypointId = 0;
            else if (currentWaypointId >= waypoints.Length)
            {
                currentWaypointId = waypoints.Length - 1;
                patrolDirection = -1;
            }
            else if (currentWaypointId < 0)
            {
                currentWaypointId = 0;
                patrolDirection = 1;
            }
            if (process.Canceled) break;
        }
    }

    private async void PatrolRoutineGuard(AsyncProcessId process)
    {
        float waitTime = 1;
        while (process.valid)
        {
            await TurnToTarget(process, startPosition + startRotation * Vector3.forward, patrolTurnSpeed);
            if (process.Canceled) break;
            await TurnToTarget(process, startPosition + startRotation * (Vector3.right + Vector3.forward*0.2f), patrolTurnSpeed);
            if (process.Canceled) break;
            await Task.Delay(System.TimeSpan.FromSeconds(waitTime));
            if (process.Canceled) break;
            await TurnToTarget(process, startPosition + startRotation * Vector3.forward, patrolTurnSpeed);
            if (process.Canceled) break;
            await TurnToTarget(process, startPosition + startRotation * (Vector3.left + Vector3.forward * 0.2f), patrolTurnSpeed);
            if (process.Canceled) break;
            await Task.Delay(System.TimeSpan.FromSeconds(waitTime));
            if (process.Canceled) break;
        }
    }

    #endregion

    #region DETECTION

    [Header("Detection")]
    public float detectionRange = 8;
    public float detectionCloseRange = 1;
    public float detectionAngle = 30;
    public float detectionHeightLimit = 4;
    private GameObject player;

    private bool IsPlayerInDetectionArea()
    {
        Vector3 toPlayer = player.transform.position - transform.position;
        if (toPlayer.magnitude <= detectionRange)
        {
            float heightDiff = Mathf.Abs(toPlayer.y);
            if (heightDiff <= detectionHeightLimit)
            {
                toPlayer.y = 0;
                float angle = Vector3.Angle(transform.forward, toPlayer.normalized);
                if (angle <= detectionAngle || toPlayer.magnitude <= detectionCloseRange)
                {
                    Ray ray = new Ray(transform.position, toPlayer);
                    if (!Physics.Raycast(ray, toPlayer.magnitude, LayerMask.GetMask("Default")))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private async Task<bool> PlayerDetection(AsyncProcessId process)
    {
        if (!player) player = FindObjectOfType<Player>().gameObject;

        float deltaTime = 0.5f; // only try to detect player every x seconds
        float timeBefore = 2;

        while (process.valid)
        {
            if (GameManager.main.GetCurrentGamePhase() == GamePhase.Spirit)
            {
                await Task.Delay(System.TimeSpan.FromSeconds(timeBefore));
                if (process.Canceled) return false;
                while (process.valid)
                {
                    if (IsPlayerInDetectionArea()) return true;
                    // Player is NOT detected
                    await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
                    if (process.Canceled) return false;
                    if (GameManager.main.GetCurrentGamePhase() == GamePhase.Real)
                    {
                        await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
                        if (process.Canceled) return false;
                        break;
                    }
                }
            }
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
            if (process.Canceled) return false;
        }
        return false;
    }

    private async void TransitionToAttack(AsyncProcessId process)
    {
        //Log("Player detection start");
        if (await PlayerDetection(process))
        {
            //Log("Player detected");
            process.Cancel();
            mainProcess = mainProcess.GetNew();
            alerted = true;
            State_Attack(mainProcess);
        }
    }

    #endregion

    #region ATTACK

    [Header("Attack")]
    public bool alwaysSeePlayer = false;
    public float attackSpeed = 7;
    public float attackTurnSpeed = 80;
    public float attackRange = 2;
    private bool alerted = false;
    public float attackShortMemory = 4;

    private static bool playerWasHit = false;

    private async void PerformAttack(AsyncProcessId process)
    {
        if (alwaysSeePlayer)
        {
            Log("Player was hit by Nemesis");
            // Call GameManager Event here!
            GameManager.main.PlayerGotHit(1, this.gameObject);
            process.Cancel();
            transform.position = posWhenPerformedAttack;
            transform.LookAt(player.transform.position - new Vector3(0, 1f, 0), Vector3.up);
            await Task.Delay(System.TimeSpan.FromSeconds(1));
            return;
        }
        if (playerWasHit || GameManager.main.GetCurrentGamePhase() == GamePhase.Real)
        {
            process.Cancel();
            return;
        }
        Log("Player was hit by Enemy");
        SoundsLibrary.PlaySoundEffect("DemonBite");
        playerWasHit = true;
        GameManager.main.PlayerGotHit(1, this.gameObject);
        process.Cancel();
        mainProcess = mainProcess.GetNew();
        transform.position = posWhenPerformedAttack;
        transform.LookAt(player.transform.position - new Vector3(0, 0.4f, 0), Vector3.up);
        await Task.Delay(System.TimeSpan.FromSeconds(1));
        playerWasHit = false;
        ResetAll();
    }

    private async void PlayerInAttackRange(AsyncProcessId process)
    {
        float deltaTime = 0.1f;

        while (process.valid)
        {
            if (playerWasHit || (GameManager.main.GetCurrentGamePhase() == GamePhase.Real && !alwaysSeePlayer))
            {
                return;
            }
            Vector3 toPlayer = player.transform.position - transform.position;

            if (Mathf.Abs(toPlayer.y) < detectionHeightLimit)
            {
                toPlayer.y = 0;
                if (toPlayer.magnitude <= attackRange)
                {
                    posWhenPerformedAttack = transform.position;
                    PerformAttack(process);
                    return;
                }
            }
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
            if (process.Canceled) break;
        }
    }

    private async Task<bool> LosePlayer(AsyncProcessId process)
    {
        const float deltaTime = 1f; // only try to detect player every x seconds

        while (process.valid)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
            if (playerWasHit || GameManager.main.GetCurrentGamePhase() == GamePhase.Real)
            {
                return true;
            }
            if (!IsPlayerInDetectionArea()) return true;
            if (process.Canceled) break;
            // Player is detected
        }
        return false;
    }

    private async void TransitionToSearch(AsyncProcessId process)
    {
        const float waitTime = 2;
        if (await LosePlayer(process))
        {
            //process.Cancel();
            await Task.Delay(System.TimeSpan.FromSeconds(attackShortMemory));
            mainProcess = mainProcess.GetNew();
            alerted = false;
            currentWaypointId = 1;
            State_Search(mainProcess);
        }
    }

    private async void LookAtPlayer(AsyncProcessId process)
    {
        while (process.valid)
        {
            await TurnToTarget(process, player.transform.position, attackTurnSpeed);
            if (process.Canceled) break;
        }
    }

    private async void State_Attack(AsyncProcessId process)
    {
        //Log("Attack start");
        alerted = true;
        SoundsLibrary.PlayOneShotNoOverride("DemonAlert");
        AlertPeers();
        PlayerInAttackRange(process);
        TransitionToSearch(process);
        //LookAtPlayer(process);
        while (process.valid)
        {
            TurnToTarget(process, player.transform.position, attackTurnSpeed);
            await MoveToTarget(process, player.transform.position, attackSpeed);
            if (process.Canceled) break;
        }
        //Log("Attack end");
    }

    private async void State_PermanentAttack(AsyncProcessId process)
    {
        if (!player) player = FindObjectOfType<Player>().gameObject;
        await Task.Delay(System.TimeSpan.FromSeconds(2)); // Wait 2 seconds before pursuit
        PlayerInAttackRange(process);
        while (process.valid)
        {
            TurnToTarget(process, player.transform.position, attackTurnSpeed);
            await MoveToTarget(process, player.transform.position, attackSpeed);
            if (process.Canceled) break;
        }
        Log("Nemesis Canceled: Process Invalid");
        //GameManager.main.PlayerGotHit(1, this.gameObject);
        //this.gameObject.SetActive(false);
    }

    #endregion ATTACK

    #region SEARCH

    [Header("Search")]
    public float searchTime = 5;

    private async void TransitionToPatrol(AsyncProcessId process)
    {
        await Task.Delay(System.TimeSpan.FromSeconds(searchTime));
        DirectTransitionToPatrol(process);
    }

    private async void DirectTransitionToPatrol(AsyncProcessId process)
    {
        if (process.Canceled) return;
        mainProcess = mainProcess.GetNew();
        TurnToTarget(mainProcess, startPosition, patrolTurnSpeed);
        TransitionToAttack(mainProcess);
        await MoveToTarget(mainProcess, startPosition, patrolSpeed);
        currentWaypointId = 0;
        await WaitForPeersToPatrol(mainProcess);
        mainProcess = mainProcess.GetNew();
        State_Patrol(mainProcess);
    }

    private async void State_Search(AsyncProcessId process)
    {
        alerted = false;
        SoundsLibrary.PlayOneShotNoOverride("DemonSearch", transform, 0.7f);
        TransitionToAttack(process);
        TransitionToPatrol(process);
        Vector3 right = transform.position + transform.right;
        Vector3 left = transform.position - transform.right;
        while (process.valid)
        {
            await TurnToTarget(process, right, patrolTurnSpeed);
            await TurnToTarget(process, left, patrolTurnSpeed);
            if (process.Canceled) break;
        }
    }

    #endregion

    #region RELATIONSHIPS

    [Header("Relationships")]
    public EnemyBehaviour[] peersToAlert;
    public EnemyBehaviour[] peersToWaitForPatrol;

    public async void Alert()
    {
        //return;
        alerted = true;
        mainProcess = mainProcess.GetNew();
        await TurnToTarget(mainProcess, player.transform.position, attackTurnSpeed);
        //await MoveToTarget(mainProcess, player.transform.position, attackSpeed);
        State_Attack(mainProcess);
    }

    private void AlertPeers()
    {
        //return;
        if (!alerted || playerWasHit || GameManager.main.GetCurrentGamePhase() == GamePhase.Real) return;
        foreach (var peer in peersToAlert)
        {
            if (peer == null) continue;
            if (!peer.alerted) peer.Alert();
        }
    }

    private async Task<bool> WaitForPeersToPatrol(AsyncProcessId process)
    {
        //return true;
        if (peersToWaitForPatrol.Length == 0) return true;

        float deltaTime = 1f;

        //TransitionToAttack(process);

        while (process.valid)
        {
            bool canPatrol = true;
            foreach (var peer in peersToWaitForPatrol)
            {
                if (peer == null) continue;
                if (peer.currentWaypointId != 0) canPatrol = false;
            }

            if (canPatrol) return true;

            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
        }
        return false;
    }

    #endregion

    #region GIZMO

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Quaternion rightDetectionRotation = Quaternion.Euler(transform.rotation.eulerAngles + Vector3.up * detectionAngle);
        Quaternion leftDetectionRotation = Quaternion.Euler(transform.rotation.eulerAngles - Vector3.up * detectionAngle);
        Gizmos.DrawLine(transform.position, transform.position + rightDetectionRotation * Vector3.forward * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + leftDetectionRotation * Vector3.forward * detectionRange);
        Gizmos.color = Color.white;
    }

    #endregion
}
