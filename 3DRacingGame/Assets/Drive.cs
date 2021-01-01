using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive : MonoBehaviour
{
    public WheelCollider[] wcs;
    public GameObject[] wheels;
    public float torque = 200;
    public float maxSteerAngle = 30;
    public float maxBrakeTorque = 500;

    public AudioSource skidSound;
    public AudioSource highAccel;

    public Transform SkidTrailPrefab;
    Transform[] skidTrails = new Transform[4];
   
    public ParticleSystem smokePrefab;
    ParticleSystem[] skidSmoke = new ParticleSystem[4];

    public GameObject brakeLight;

    public Rigidbody rb;
    public float gearLength = 3;
    public float currentSpeed { get { return rb.velocity.magnitude * gearLength; } }

    public float lowPitch = 1f;
    public float highPitch = 6f;
    public int numGears = 5;
    //float rpm;
    //int currentGear = 1;
    //float currentGearPerc;
    public float maxSpeed = 200f;

    int gearStatus = 1;
    float gearAmount = 50;
    [SerializeField] GameObject smokeEffect;


    void Start()
    {
       for(int i= 0; i<4; i++)
        {
            skidSmoke[i] = Instantiate(smokePrefab);
            skidSmoke[i].Stop();
        }
        brakeLight.SetActive(false);
    }
    void Update()
    {
        float a = Input.GetAxis("Vertical");
        float s = Input.GetAxis("Horizontal");
        float b = Input.GetAxis("Jump");
        Go(a , s , b);
        SetSound();
       // SetSmoke();
       // print(currentSpeed);

        CheckForSkid();
        //CalculateEngineSound();
    }
    void Go(float accel , float steer, float brake)
    {
        accel = Mathf.Clamp(accel, -1, 1);
        steer = Mathf.Clamp(steer, -1, 1) * maxSteerAngle;
        brake = Mathf.Clamp(brake, 0, 1) * maxBrakeTorque;

        //SetStopLight(brake);

        float thrussTorque = 0;
        if (currentSpeed < maxSpeed)
        {
            thrussTorque = accel * torque;
        }

        for (int i = 0; i < 4; i++)
        {
            wcs[i].motorTorque = thrussTorque;
            if (i < 2)
            {
                wcs[i].steerAngle = steer;
                wcs[i].brakeTorque = brake;
            }
            else
            {
               // wcs[i].brakeTorque = brake;
            }

            Quaternion quat;
            Vector3 position;
            wcs[i].GetWorldPose(out position, out quat);
            wheels[i].transform.position = position;
            wheels[i].transform.rotation = quat;
        }
    }


   


    private void SetStopLight(float brake)
    {
        if (brake != 0)
        {
            brakeLight.SetActive(true);
        }
        else
        {
            brakeLight.SetActive(false);
        }
    }


    void SetSound()
    {
        float speedRatio = (currentSpeed / gearAmount) + 0.6f;
       
      
        if(speedRatio < 1.6f)
        {
            highAccel.pitch = speedRatio;
        }else if(gearStatus < 5)
        {
            gearStatus++;
            gearAmount += 50;
        }
    } // Fix gear down mechanism

   



    //void CalculateEngineSound()
    //{
    //    float gearPercentage = (1 / (float)numGears);
    //    float targetGearFactor = Mathf.InverseLerp(gearPercentage * currentGear, gearPercentage * (currentGear + 1), Mathf.Abs(currentSpeed / maxSpeed));
    //    currentGearPerc = Mathf.Lerp(currentGearPerc, targetGearFactor, Time.deltaTime * 5f);

    //    var gearNumFactor = currentGear / (float) numGears;
    //    rpm = Mathf.Lerp(gearNumFactor, 1, currentGearPerc);
    //    float speedPercentage = Mathf.Abs(currentSpeed / maxSpeed);
    //    float upperGearMax = (1 / (float)numGears) * (currentGear + 1);
    //    float downGearMax = (1 / (float)numGears) * currentGear;

    //    if(currentGear>0 && speedPercentage< downGearMax)
    //    {
    //        currentGear--;
    //    } 
    //    if(speedPercentage > upperGearMax && (currentGear < (numGears - 1)))
    //    {
    //        currentGear++;
    //    }

    //    float pitch = Mathf.Lerp(lowPitch, highPitch, rpm);
    //    highAccel.pitch = Mathf.Min(highPitch, pitch) * 0.25f;
    //}
   
    
    void CheckForSkid()
    {
        int numSkidding = 0;
        for(int i = 0; i<4; i++)
        {
            WheelHit wheelHit;
            wcs[i].GetGroundHit(out wheelHit);
            if(Mathf.Abs(wheelHit.forwardSlip) >= 0.7f || Mathf.Abs(wheelHit.sidewaysSlip) >= 0.9f)
            {
                numSkidding++;
                if (!skidSound.isPlaying)
                {
                    skidSound.Play();
                }
                StartSkidTrail(i);
              
                skidSmoke[i].transform.position = wcs[i].transform.position - wcs[i].transform.up * wcs[i].radius;
                skidSmoke[i].Emit(1);
            }
            else
            {
                EndSkidTrail(i);
              
            }
        }

        if(numSkidding == 0 && skidSound.isPlaying)
        {
            skidSound.Stop();
         
        }
    }
    public void StartSkidTrail(int i)
    {
        if(skidTrails[i] == null)
        {
            skidTrails[i] = Instantiate(SkidTrailPrefab);
        }
        skidTrails[i].parent = wcs[i].transform;
        skidTrails[i].localPosition = -Vector3.up * wcs[i].radius;
        skidTrails[i].localRotation = Quaternion.Euler(90, 0, 0);
    }
    public void EndSkidTrail(int i)
    {
        if (skidTrails[i] == null) return;
        Transform holder = skidTrails[i];
        skidTrails[i] = null;
        holder.parent = null;
        holder.rotation = Quaternion.Euler(90, 0, 0);
        Destroy(holder.gameObject, 30f);
    }



   

}
