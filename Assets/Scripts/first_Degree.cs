using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using static System.Console;
using System.Threading;
using System;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using MQTTnet.Client;
using MQTTnet.Packets;
using MQTTnet.Server;
using MQTTnet;
using System.Threading.Tasks;

public class first_Degree : MonoBehaviour
{
    ArticulationBody Articulation_Body = null;
    private float Lower_Limit = 0f;
    private float Upper_Limit = 0f;
    private float Stifnees = 0f;
    private float Damping = 0f;
    private float Force_Limit = 10f;
    private float Target = 0f;
[SerializeField]   private static float target_angle = 0f;
    private float Target_Velocity = 0f;
    private int counter = 0;


    private  ArticulationDrive Xdrive;

    public IMqttClient mqttClient=null;
    async void Awake()
    {
        //GameObject.Find("Targetx").GetComponent<type>();
        // publish_connect();
        await MqttConnect();
        subscribe();
    }
    // Start is called before the first frame update
    void Start()
    {
        // Articulation_Body=this.GetComponent<ArticulationBody>();
        Articulation_Body = this.GetComponent<ArticulationBody>();
        UnityThread.initUnityThread();
        //move_Shoulder_Link();
        //Invoke("move_Shoulder_Link",1);
       //InvokeRepeating("move_Shoulder_Link", 1, 1);
    }
     void move_Shoulder_Link(string Msg)
    {
        Xdrive = Articulation_Body.xDrive;
        target_angle = float.Parse(Msg);
        Debug.Log("floatangle" + target_angle);
        Xdrive.target = target_angle;
        Xdrive.targetVelocity = 10f;
        Articulation_Body.xDrive = Xdrive;
    }
    async Task MqttConnect()
    {
        var mqttFactory = new MqttFactory();

        mqttClient = mqttFactory.CreateMqttClient();

        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("broker.hivemq.com").Build();

        var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
        try
        {
            await mqttClient.PingAsync(CancellationToken.None);
            Debug.Log("Connected");
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
        await Task.Delay(100);
        Add_RecieveMsg_Handler();
    }
    async void subscribe()
    {
        await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("shoulder").Build());
        Debug.Log("done subscribing");
    }
    void Add_RecieveMsg_Handler()
    {
        mqttClient.ApplicationMessageReceivedAsync += async arg =>
        {
           
            var Msg = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
            
            try {
                Debug.Log("**" + Msg);
                UnityThread.executeInFixedUpdate(() => move_Shoulder_Link(Msg));
            }
            catch (Exception ex)    
            {
                Debug.Log(ex);
            }
            await Task.Delay(100);
        };
    }

    /* void publish_connect() {
          MqttClient mqttclient;
             mqttclient = new MqttClient("broker.hivemq.com");
             mqttclient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;
             mqttclient.Subscribe(new string[] {"shoulder"}, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
             mqttclient.Connect("tarek");

         }*/
    /*static private void MqttClient_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
    {
         var message = Encoding.UTF8.GetString(e.Message);
        Debug.Log(message);
        target_angle=float.Parse(message);
    }*/



}

