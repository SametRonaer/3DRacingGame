using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public Circuit circuit;
    public float brakingSensitivity = 3f;
    Drive ds;
    bool wpReady;
    public float steeringSensitivity = 0.01f; // orjinal 0.3 ancak ben 0.01 kullanıyorum
    public float accelSensitivity = 0.3f;
    Vector3 target;
    Vector3 nextTarget;
    int currentWP = 0;
    float totalDistanceToTarget;

    // Start is called before the first frame update
    void Start()
    {
        ds = GetComponent<Drive>();
        Invoke("WaypointsReady", 0.5f);
        
       
    }

    // Update is called once per frame
    bool isJump = false;
    void Update()
    {
        if (wpReady)
        {
            Vector3 localTarget = ds.rb.gameObject.transform.InverseTransformPoint(target);
            Vector3 nextLocalTarget = ds.rb.gameObject.transform.InverseTransformPoint(nextTarget);
            float distanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position); // Her frame'de target'a olan uzaklık yeniden hesaplanıyor

            float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
            float nextTargetAngle = Mathf.Atan2(nextLocalTarget.x, nextLocalTarget.z) * Mathf.Rad2Deg; // Dönüş gerektiren yerlerde daha çok fren yapmak için kullanıyoruz

            float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(ds.currentSpeed);

            float distanceFactor = distanceToTarget / totalDistanceToTarget; // İki waypoint arası mesafeyi aracın sonraki waypointe'e olan uzaklığına bölerek yüzde hesaplıyoruz
            float speedFactor = ds.currentSpeed / ds.maxSpeed; // hızımıza göre frene basma oranımızı ayarlamak için kullanıyoruz

            //float accel = 1f;
            float accel = Mathf.Lerp(accelSensitivity, 1, distanceFactor);
            // Orjinal kod açıyı kullanıyor ancak ben kullanmadım ayrıca orjinal breakinSesitivity = 1
            //float brake = Mathf.Lerp((-1 -Mathf.Abs(nextTargetAngle) * brakingSensitivity) ,1+speedFactor, 1 - distanceFactor); // waypoint'e yaklaştıkça ve viraj açısı arttıkça frene basma ölçüsünü arttırıyoruz
           float brake = Mathf.Lerp((-1  * brakingSensitivity) ,1+speedFactor, 1 - distanceFactor); // waypoint'e yaklaştıkça ve viraj açısı arttıkça frene basma ölçüsünü arttırıyoruz


            if(Mathf.Abs(nextTargetAngle)> 20)
            {
                brake += 0.8f;
                accel -= 0.8f;
            }

            if (isJump)
            {
                accel = 1;
                brake = 0;
                Debug.Log("Jump");
            }

           // Debug.Log("Brake: "+ brake+ " Accel: "+ accel + " Speed: "+ ds.rb.velocity.magnitude);

            //if(distanceToTarget< 5)
            //{
            //    brake = 0.8f;
            //    accel = 0.1f;
            //}

            ds.Go(accel, steer, brake);

            if (distanceToTarget < 4) // Threshhol make large if car starts to circle waypoint
            {
                currentWP++;
                if (currentWP >= circuit.waypoints.Length)
                {
                    currentWP = 0;
                }
                target = circuit.waypoints[currentWP].transform.position;
                if(currentWP == circuit.waypoints.Length - 1)
                {

                nextTarget = circuit.waypoints[0].transform.position;
                }
                else
                {
                nextTarget = circuit.waypoints[currentWP + 1].transform.position;

                }
                totalDistanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position); // Sonraki waypoint için yüzde hesaplamak için kullanacağız her waypoint'i geçince yeniden hesaplanıyor
            
            if(ds.rb.gameObject.transform.InverseTransformPoint(target).y > 5)
                {
                    isJump = true;
                }
                else
                {
                    isJump = false;
                }

            }
            ds.CheckForSkid();
            ds.CalculateEngineSound();
        }
      
    }

    void WaypointsReady()
    {
        target = circuit.waypoints[currentWP].transform.position;
        nextTarget = circuit.waypoints[currentWP + 1].transform.position;
        totalDistanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position);
        wpReady = true;
    }
}
