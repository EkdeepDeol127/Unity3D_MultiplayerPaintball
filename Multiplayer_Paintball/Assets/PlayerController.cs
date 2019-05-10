using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : NetworkBehaviour
{
    public GameObject[] spawnPoints;
    public GameObject Bullet;
    public Transform BulletSpawnPoint;
    private Text DisplayText;
    private Text AmmoText;
    private Text RedText;
    private Text BlueText;
    private Text GreenText;
    private int RedScore = 0;
    private int BlueScore = 0;
    private int GreenScore = 0;
    private float speed = 5.0f;
    private Rigidbody m_rb = null;
    private Color m_startingColour;
    private Color m_HitColour;
    private float attackCooldownTime = 2.0f;
    private float currentAttackTimer = -1.0f;
    private float timer = 1;
    private bool GameStart = false;
    public bool hasPlayers = false;
    private bool once = false;
    private int Ammo = 30;
    
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();

        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (Vector3.Distance(spawnPoints[i].GetComponent<SpawnPointScript>().SpawnLocation.position, transform.position) < 10)
            {
                m_startingColour = spawnPoints[i].GetComponent<SpawnPointScript>().color.color;
                GetComponent<Renderer>().material.color = m_startingColour;
                break;
            }
        }
        if (hasAuthority)
        {
            if(isServer)
            {
                NetworkServer.RegisterHandler(MsgDetails.MsgId, DetailsMsg);
                NetworkServer.RegisterHandler(MsgDestroy.MsgId, serverReceiveObject);
                NetworkServer.RegisterHandler(MsgStats.MsgId, ReceivedColor);
            }
            else if(isClient)
            {
                CustomNetworkManager.singleton.client.RegisterHandlerSafe(MsgDetails.MsgId, DetailsMsg);
                CustomNetworkManager.singleton.client.RegisterHandlerSafe(MsgStats.MsgId, ReceivedColor);
            }

            Debug.Log("Start Color: " + m_startingColour);

            Text[] tempText = FindObjectsOfType<Text>();
            for (int i = 0; i < tempText.Length; i++)
            {
                switch (tempText[i].tag)
                {
                    case "DisplayText":
                        DisplayText = tempText[i];
                        break;
                    case "AmmoText":
                        AmmoText = tempText[i];
                        break;
                    case "RedText":
                        RedText = tempText[i];
                        RedText.text = "Red Score: " + RedScore;
                        break;
                    case "BlueText":
                        BlueText = tempText[i];
                        BlueText.text = "Blue Score: " + BlueScore;
                        break;
                    case "GreenText":
                        GreenText = tempText[i];
                        GreenText.text = "Green Score: " + GreenScore;
                        break;
                    default:
                        break;
                }
            }

            PlayerController[] tempPC = FindObjectsOfType<PlayerController>();
            if (tempPC.Length == 3)
            {
                AmmoText.text = "Ammo: " + Ammo;
                SendMsg();
            }
            else if (tempPC.Length < 3)
            {
                DisplayText.text = "Waiting For Players";
            }
            else if (tempPC.Length > 3)
            {
                DisplayText.text = "Game Is Full";
            }
        }
    }

    //striclty for replicas
    void ReplicaUpdate()
    {
        UpdateAttack();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Bullet")
        {
            Debug.Log("Bullet Color" + Bullet.GetComponent<BulletBehavior>().m_HoldColor);
            Debug.Log("Start Color: " + m_startingColour);
        }
        if (other.CompareTag("Bullet") && other.GetComponent<BulletBehavior>().m_HoldColor != m_startingColour)
        {
            m_HitColour = other.GetComponent<BulletBehavior>().m_HoldColor;
            if (hasAuthority)//make delay for color to reset to startColor
            {
                if (m_HitColour.r > 0)
                {
                    RedScore++;
                    RedText.text = "Red Score: " + RedScore;
                    sendColorScore();
                }
                if (m_HitColour.b > 0)
                {
                    BlueScore++;
                    BlueText.text = "Blue Score: " + BlueScore;
                    sendColorScore();
                }
                if (m_HitColour.g > 0)
                {
                    GreenScore++;
                    GreenText.text = "Green Score: " + GreenScore;
                    sendColorScore();
                }
            }
            if (isServer)
            {
                RpcAttack();
            }
            else if(isClient)
            {
                CmdAttack();
            }
        }
        if(other.CompareTag("PickUp"))
        {
            Ammo = 30;
            AmmoText.text = "Ammo: " + Ammo;
            if(hasAuthority)
            {
                if(isServer)
                {
                    Destroy(other.gameObject);
                    NetworkServer.Destroy(other.gameObject);
                }
                else if(isClient)
                {
                    sendObjDestroy(other.GetComponent<NetworkIdentity>());
;               }
            }
        }
    }

    void SendMsg()//for start
    {
        MsgDetails MyMsg = new MsgDetails();
        MyMsg.Ready = true;

        if (isServer)
        {
            NetworkServer.SendToAll(MsgDetails.MsgId, MyMsg);
        }
        else
        {
            CustomNetworkManager.singleton.client.Send(MsgDetails.MsgId, MyMsg);
        }
    }

    protected void DetailsMsg(NetworkMessage msg)//for start
    {
        MsgDetails MyMsg = msg.ReadMessage<MsgDetails>();       
        hasPlayers = MyMsg.Ready;  
        if(isServer)//so server can relay to clients
        {
            SendMsg();
        }
    }

    void sendObjDestroy(NetworkIdentity ObjID)//for pickup sent by server
    {
        MsgDestroy MyMsg = new MsgDestroy();
        MyMsg.netId = ObjID.netId;

        if (isClient)
        {
            CustomNetworkManager.singleton.client.Send(MsgDestroy.MsgId, MyMsg);
        }
    }

    void serverReceiveObject(NetworkMessage msg)// for pickup received by client
    {
        MsgDestroy MyMsg = msg.ReadMessage<MsgDestroy>();
        if (isServer)
        {
            GameObject temp = NetworkServer.FindLocalObject(MyMsg.netId);
            Destroy(temp);
            NetworkServer.Destroy(temp);
        }
    }

    void sendColorScore()//for color
    {
        MsgStats MyMsg = new MsgStats();
        MyMsg.ColorRed = RedScore;
        MyMsg.ColorBlue = BlueScore;
        MyMsg.ColorGreen = GreenScore;

        if (isServer)
        {
            NetworkServer.SendToAll(MsgStats.MsgId, MyMsg);
        }
        else
        {
            CustomNetworkManager.singleton.client.Send(MsgStats.MsgId, MyMsg);
        }
    }

    void ReceivedColor(NetworkMessage msg)//for color
    {
        MsgStats MyMsg = msg.ReadMessage<MsgStats>();
        if (MyMsg.ColorRed > RedScore)
        {
            RedScore = MyMsg.ColorRed;
            RedText.text = "Red Score: " + RedScore;
        }
        if(MyMsg.ColorBlue > BlueScore)
        {
            BlueScore = MyMsg.ColorBlue;
            BlueText.text = "Blue Score: " + BlueScore;
        }
        if(MyMsg.ColorGreen > GreenScore)
        {
            GreenScore = MyMsg.ColorGreen;
            GreenText.text = "Green Score: " + GreenScore;
        }
        if (isServer)//so server can relay to clients for sync
        {
            sendColorScore();
        }
    }

    [Command]
    void CmdAttack()
    {
        //local attack
        Attack();
        //broadcast attack
        if (isServer)
        {
            RpcAttack();
        }
    }

    [ClientRpc]
    void RpcAttack()
    {
        //local attack
        Attack();
    }

    void Attack()
    {
        if (currentAttackTimer < 0.0f)
        {
            currentAttackTimer = attackCooldownTime;
        }
    }

    [Command]
    void CmdFire()
    {
        GameObject temp = Instantiate(Bullet, BulletSpawnPoint.position, BulletSpawnPoint.rotation);
        temp.GetComponent<BulletBehavior>().m_HoldColor = m_startingColour;
        Debug.Log("Bullet Color" + Bullet.GetComponent<BulletBehavior>().m_HoldColor);
        if (isServer)
        {
            NetworkServer.Spawn(temp);
        }
    }

    void UpdateAttack()
    {
        if (currentAttackTimer < 0.0f)
        {
            return;
        }

        Color color = GetComponent<Renderer>().material.color;
        Vector3 sourceColour = new Vector3(m_HitColour.r, m_HitColour.g, m_HitColour.b);
        Vector3 destColour = new Vector3(m_startingColour.r, m_startingColour.g, m_startingColour.b);
        currentAttackTimer -= Time.deltaTime;
        float ratio = 1.0f - Mathf.Clamp(currentAttackTimer / attackCooldownTime, 0.0f, 1.0f);
        Vector3 vColour = Vector3.Lerp(sourceColour, destColour, ratio);
        color.r = vColour.x;
        color.g = vColour.y;
        color.b = vColour.z;
        GetComponent<Renderer>().material.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasAuthority)
        {
            ReplicaUpdate();
            return;
        }

        if (GameStart == true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isServer)
                {
                    RpcAttack();
                }
                else
                {
                    CmdAttack();
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if(Ammo > 0)
                {
                    Ammo--;
                    AmmoText.text = "Ammo: " + Ammo;
                    CmdFire();
                }
            }

            Vector3 forwardVelocity = transform.forward * Input.GetAxis("Vertical") * speed;
            Vector3 strafeVelocity = transform.up * Input.GetAxis("Horizontal") * speed;
            if (forwardVelocity.z != 0)
            {
                m_rb.velocity = forwardVelocity;
            }
            if (strafeVelocity.y != 0)
            {
                transform.Rotate(strafeVelocity);
            }
            UpdateAttack();
        }

        if (timer <= 0 && GameStart == false && once == false)
        {
            GetComponentInChildren<Camera>().enabled = true;
            GameStart = true;
            once = true;
            timer = 420;
        }
        else if (hasPlayers == true && timer > 0)
        {
            timer -= Time.deltaTime;
            DisplayText.text = "Time: " + Mathf.RoundToInt(timer).ToString();
        }
        else if(timer <= 0 && GameStart == true && once == true)
        {
            DisplayText.text = "GAME OVER";
            m_rb.velocity = Vector3.zero;
            GameStart = false;
        }
    }
}

public class MsgDetails : MessageBase
{
    public static short MsgId = MsgType.Highest + 1;
    public static float SendTime = 0.5f;
    public MsgDetails()
    {

    }
    public bool Ready;
}

public class MsgDestroy : MessageBase
{
    public static short MsgId = MsgType.Highest + 2;
    public static float SendTime = 0.5f;
    public MsgDestroy()
    {

    }

    public NetworkInstanceId netId;
}

public class MsgStats : MessageBase
{
    public static short MsgId = MsgType.Highest + 3;
    public static float SendTime = 0.5f;
    public MsgStats()
    {

    }

    public int ColorRed = 0;
    public int ColorBlue = 0;
    public int ColorGreen = 0;
}