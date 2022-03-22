using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class cameraMovementController : MonoBehaviour
{
    private Transform cameraT;


    private Transform CT;
    private Transform Kuwait;
    private Transform Ireland;
    private Transform Australia;
    private Transform Brazil;
    private Transform Peru;
    private Transform China;

    private bool moveLocked = false; 
    public float speed = 16f;
    public Camera mainCamera;
    public TextMeshProUGUI numberText;
    public Image dialedFlag;
    public string currentlyDialed;
    

    public Dictionary<string, Transform> destDictionary = new Dictionary<string, Transform>();


    void Start(){
        cameraT = gameObject.GetComponent<Transform>();
        CT = GameObject.Find("CTMarker").GetComponent<Transform>();
        Ireland = GameObject.Find("IrelandMarker").GetComponent<Transform>();
        Kuwait = GameObject.Find("KuwaitMarker").GetComponent<Transform>();
        Australia = GameObject.Find("AustraliaMarker").GetComponent<Transform>();
        Brazil = GameObject.Find("BrazilMarker").GetComponent<Transform>();
        Peru = GameObject.Find("PeruMarker").GetComponent<Transform>();
        China = GameObject.Find("ChinaMarker").GetComponent<Transform>();


        mainCamera = gameObject.GetComponent<Camera>();
        numberText = GameObject.Find("DialedNumber").GetComponent<TextMeshProUGUI>();
        dialedFlag = GameObject.Find("DialedFlag").GetComponent<Image>();

        //Add values to dictionary
        destDictionary.Add("203", CT);
        destDictionary.Add("351", Ireland);
        destDictionary.Add("965", Kuwait);
        destDictionary.Add("610", Australia);
        destDictionary.Add("550", Brazil);
        destDictionary.Add("510", Peru);
        destDictionary.Add("860", China);

        //Starting position
         cameraT.position = new Vector3(CT.position.x, CT.position.y, -10f);

         //Starting Text
         numberText.text = "Dial";
         

    }

    IEnumerator processDial(int numDialed){
        moveLocked = true;
        bool triggerCode = false;
        if(numberText.text.Length >= 30){
            currentlyDialed = numDialed.ToString();
        }else if(numberText.text.Length < 29){
            currentlyDialed += numDialed.ToString();
        }else if(numberText.text.Length == 29){
            currentlyDialed += numDialed.ToString();
            triggerCode = true;
        }
        numberText.alignment = TextAlignmentOptions.Left;
        numberText.text = "<mspace=mspace=80>" + currentlyDialed + "</mspace>";

        yield return new WaitForSeconds(0.5f);
        
        if(!triggerCode){
            moveLocked = false;
            //Going to need to unlock after transition and audio player
        }
        moveLocked = false;

        if(triggerCode){ //Slide out dial bar and start the visual transition
            //Heres where I need to set the destination and the slide
            Vector3 dest = new Vector3(0,0,0);
            bool found = false;
            if(destDictionary.ContainsKey(currentlyDialed)){
                dest = destDictionary[currentlyDialed].position;
                found = true;
                Debug.Log("Found the position maybe");
            }else{
                Debug.Log("Code not valid");
            }
            
            if(found){
                Debug.Log("Starting slide");
                Transform DialBar = GameObject.Find("DialBar").GetComponent<Transform>();
                Vector3 StowedBarTarget = new Vector3(5, 350, 0);
                Vector3 OpenBarTarget = new Vector3(5, 210, 0);
                //Top bar leaves
                while(DialBar.localPosition != StowedBarTarget){
                    DialBar.localPosition = Vector3.MoveTowards(DialBar.localPosition, StowedBarTarget, 256f * Time.deltaTime);
                    yield return null;
                }

                Vector3 cameraDest = new Vector3(dest.x, dest.y, -10f);

                //Camera moves to destination
                while(cameraT.position != cameraDest){
                    cameraT.position = Vector3.MoveTowards(cameraT.position, cameraDest, speed * Time.deltaTime);
                    yield return null;
                }

                //Top bar comes back
                while(DialBar.localPosition != OpenBarTarget){
                    //Assigns new info to the bar
                    dialedFlag.sprite = destDictionary[currentlyDialed].GetComponent<markerInfo>().flag;
                    DialBar.localPosition = Vector3.MoveTowards(DialBar.localPosition, OpenBarTarget, 256f * Time.deltaTime);
                    yield return null;
                }

                //Finds and starts audio clip
                AudioSource markerAudio = destDictionary[currentlyDialed].gameObject.GetComponent<AudioSource>();
                yield return new WaitForSeconds(0.7f);
                if(markerAudio){
                    markerAudio.Play(0);
                    Debug.Log(markerAudio.clip.length);
                    yield return new WaitForSeconds(1f);
                    while(markerAudio.time <= markerAudio.clip.length && markerAudio.time != 0){
                        yield return null;
                        //Will exit once clip is complete
                        Debug.Log(markerAudio.time);
                    }
                    yield return new WaitForSeconds(0.5f);

                    //Bar goes back up
                    while(DialBar.localPosition != StowedBarTarget){
                        DialBar.localPosition = Vector3.MoveTowards(DialBar.localPosition, StowedBarTarget, 256f * Time.deltaTime);
                        yield return null;
                    }
                    
                    //Camera returns to home destination
                    Vector3 homeTarget = new Vector3(CT.position.x, CT.position.y, cameraT.position.z);
                    while(cameraT.position != homeTarget){
                        Debug.Log("Moving Back");
                        cameraT.position = Vector3.MoveTowards(cameraT.position, homeTarget, speed * Time.deltaTime);
                        yield return null;
                    }
                    Debug.Log("Back");

                    //Bar comes back down
                    while(DialBar.localPosition != OpenBarTarget){
                        //Set back to default US flag
                        dialedFlag.sprite = CT.gameObject.GetComponent<markerInfo>().flag;
                        //Set back to default Dial Prompt
                        numberText.alignment = TextAlignmentOptions.Center;
                        numberText.text = "Dial";
                        DialBar.localPosition = Vector3.MoveTowards(DialBar.localPosition, OpenBarTarget, 256f * Time.deltaTime);
                        yield return null;
                    }
                    Debug.Log("Comes back down");

                }
                
                //Now that the clip is complete I want to go back to Connecticut

                


            }
            moveLocked = false;
            yield return null; 
        }

    
    }

    void FixedUpdate(){
        if(!moveLocked){
            if(Input.GetKey(KeyCode.Alpha1)){
                StartCoroutine(processDial(1));
            }
            else if(Input.GetKey(KeyCode.Alpha2)){
                StartCoroutine(processDial(2));
            }
            else if(Input.GetKey(KeyCode.Alpha3)){
                StartCoroutine(processDial(3));
            }
            else if(Input.GetKey(KeyCode.Alpha4)){
                StartCoroutine(processDial(4));
            }
            else if(Input.GetKey(KeyCode.Alpha5)){
                StartCoroutine(processDial(5));
            }
            else if(Input.GetKey(KeyCode.Alpha6)){
                StartCoroutine(processDial(6));
            }
            else if(Input.GetKey(KeyCode.Alpha7)){
                StartCoroutine(processDial(7));
            }
            else if(Input.GetKey(KeyCode.Alpha8)){
                StartCoroutine(processDial(8));
            }
            else if(Input.GetKey(KeyCode.Alpha9)){
                StartCoroutine(processDial(9));
            }
            else if(Input.GetKey(KeyCode.Alpha0)){
                StartCoroutine(processDial(0));
            }

        }

        
    }
}
