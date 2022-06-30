using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;

public class cameraMovementController : MonoBehaviour
{
    private Transform cameraT;

    //Transform Varaibles Used to store the location of each destination
    private Transform CT; 
    private Transform Australia;
    private Transform Kuwait;
    private Transform Philippines;
    private Transform Lebanon;
    private Transform Peru;
    private Transform Brazil;
    private Transform China;
    private Transform Ethiopia;
    private Transform Singapore;
    private Transform NewZealand;

    //Application Parameters
    private bool moveLocked = false; 
    public float speed = 16f;
    public Camera mainCamera;
    public TextMeshProUGUI numberText;
    public Image dialedFlag;
    public string currentlyDialed;

    //Lower Bar References
    private Transform lowerBar;
    private Vector3 lowerStowedTarget = new Vector3(175,-500,0);
    private Vector3 lowerBarTarget = new Vector3(175,-140,0);
    
    //Dictionary to store corrolations between dial codes and destinations
    public Dictionary<string, Transform> destDictionary = new Dictionary<string, Transform>();


    void Start(){
        cameraT = gameObject.GetComponent<Transform>(); //Grabs Camera Transform

        //Assigns each pointer to it's respective variable
        CT = GameObject.Find("CTMarker").GetComponent<Transform>(); 
        Australia = GameObject.Find("AustraliaMarker").GetComponent<Transform>();
        Kuwait = GameObject.Find("KuwaitMarker").GetComponent<Transform>();
        Philippines = GameObject.Find("PhilippinesMarker").GetComponent<Transform>();
        Lebanon = GameObject.Find("LebanonMarker").GetComponent<Transform>();
        Peru = GameObject.Find("PeruMarker").GetComponent<Transform>();
        Brazil = GameObject.Find("BrazilMarker").GetComponent<Transform>();
        China = GameObject.Find("ChinaMarker").GetComponent<Transform>();
        Ethiopia = GameObject.Find("EthiopiaMarker").GetComponent<Transform>();
        Singapore = GameObject.Find("SingaporeMarker").GetComponent<Transform>();
        NewZealand = GameObject.Find("NewZealandMarker").GetComponent<Transform>();
        

        mainCamera = gameObject.GetComponent<Camera>();
        numberText = GameObject.Find("DialedNumber").GetComponent<TextMeshProUGUI>();
        dialedFlag = GameObject.Find("DialedFlag").GetComponent<Image>();
        lowerBar = GameObject.Find("BottomBar").GetComponent<Transform>();

        //New
        //Adding necessary values to dictionary
        destDictionary.Add("610", Australia);
        destDictionary.Add("965", Kuwait);
        destDictionary.Add("630", Philippines);
        destDictionary.Add("961", Lebanon);
        destDictionary.Add("510", Peru);
        destDictionary.Add("550", Brazil);
        destDictionary.Add("860", China);
        destDictionary.Add("251", Ethiopia);
        destDictionary.Add("650", Singapore);
        destDictionary.Add("640", NewZealand);

        //Starting position in Connecticut
        cameraT.position = new Vector3(CT.position.x, CT.position.y, -10f);
        lowerBar.localPosition = lowerStowedTarget;

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

        yield return new WaitForSeconds(0.1f);
        
        if(!triggerCode){
            moveLocked = false;
            //Going to need to unlock after transition and audio player
        }
        

        if(triggerCode){ //Slide out dial bar and start the visual transition
            //Heres where I need to set the destination and the slide
            Vector3 dest = new Vector3(0,0,0);
            bool found = false;
            if(destDictionary.ContainsKey(currentlyDialed)){
                dest = destDictionary[currentlyDialed].position + new Vector3(1.5f,0,0);
                found = true;
                Debug.Log("Found the position maybe");
            }else{
                currentlyDialed = "";
                numberText.alignment = TextAlignmentOptions.Center;
                yield return new WaitForSeconds(0.5f);
                numberText.text = "Dial";
            }
            
            if(found){
                dialedFlag.sprite = destDictionary[currentlyDialed].GetComponent<markerInfo>().flag;
                VideoPlayer bottomBarVideo = GameObject.Find("BottomBar").GetComponent<VideoPlayer>();
                bottomBarVideo.clip = destDictionary[currentlyDialed].GetComponent<markerInfo>().video;
                bottomBarVideo.Play();
                bottomBarVideo.Pause();

                Debug.Log("Starting slide");
                Transform DialBar = GameObject.Find("DialBar").GetComponent<Transform>();
                Vector3 StowedBarTarget = new Vector3(5, 500, 0);
                Vector3 OpenBarTarget = new Vector3(5, 210, 0);
                //Top bar leaves
                while(DialBar.localPosition != StowedBarTarget){
                    DialBar.localPosition = Vector3.MoveTowards(DialBar.localPosition, StowedBarTarget, 256f * Time.deltaTime);
                    yield return null;
                }

                StowedBarTarget = new Vector3(175, 400, 0);
                OpenBarTarget = new Vector3(175, 210, 0);   
                DialBar.localPosition = new Vector3(175, DialBar.localPosition.y, DialBar.localPosition.z);

                Vector3 cameraDest = new Vector3(dest.x, dest.y, -10f);

                //Camera moves to destination
                while(cameraT.position != cameraDest){
                    cameraT.position = Vector3.MoveTowards(cameraT.position, cameraDest, speed * Time.deltaTime);
                    yield return null;
                }

                


                //Top bar comes back AND Bottom Bar comes in
                while(DialBar.localPosition != OpenBarTarget || lowerBar.localPosition != lowerBarTarget){
                    //Assigns new info to the bar
                    if(DialBar.localPosition != OpenBarTarget){
                        DialBar.localPosition = Vector3.MoveTowards(DialBar.localPosition, OpenBarTarget, 256f * Time.deltaTime);
                    }  
                    lowerBar.localPosition = Vector3.MoveTowards(lowerBar.localPosition, lowerBarTarget, 256f * Time.deltaTime);
                    yield return null;
                }

                //Finds and starts audio clip
                AudioSource markerAudio = destDictionary[currentlyDialed].gameObject.GetComponent<AudioSource>();
                yield return new WaitForSeconds(0.7f);
                if(markerAudio){
                    markerAudio.Play(0);
                    bottomBarVideo.Play();
                    Debug.Log(markerAudio.clip.length);
                    yield return new WaitForSeconds(1f);
                    while(markerAudio.time <= markerAudio.clip.length && markerAudio.time != 0 && !Input.GetKeyDown(KeyCode.H)){
                        //Will exit once clip is complete
                        Debug.Log("Time is:");
                        Debug.Log(markerAudio.time);
                        yield return null;
                    }
                    markerAudio.Stop();
                    yield return new WaitForSeconds(0.5f);

                    //Bar goes back up
                    while(DialBar.localPosition != StowedBarTarget || lowerBar.localPosition != lowerStowedTarget){
                        if(DialBar.localPosition != StowedBarTarget){
                            DialBar.localPosition = Vector3.MoveTowards(DialBar.localPosition, StowedBarTarget, 256f * Time.deltaTime);
                        }
                        lowerBar.localPosition = Vector3.MoveTowards(lowerBar.localPosition, lowerStowedTarget, 256f * Time.deltaTime);
                        yield return null;
                    }
                    
                    //Camera returns to home destination
                    DialBar.localPosition = new Vector3(5, DialBar.localPosition.y, DialBar.localPosition.z);
                    StowedBarTarget = new Vector3(5, 400, 0);
                    OpenBarTarget = new Vector3(5, 210, 0);
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
                    currentlyDialed = "";

                }
                
               

                


            }
            moveLocked = false;
            yield return null; 
        }

    
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.H) && currentlyDialed.Length < 3){
            currentlyDialed = "";
            numberText.alignment = TextAlignmentOptions.Center;
            numberText.text = "Dial";
        }
        if(!moveLocked){
            if(Input.GetKeyDown(KeyCode.Alpha1)){
                StartCoroutine(processDial(1));
            }
            if(Input.GetKeyDown(KeyCode.Alpha2)){
                StartCoroutine(processDial(2));
            }
            if(Input.GetKeyDown(KeyCode.Alpha3)){
                StartCoroutine(processDial(3));
            }
            if(Input.GetKeyDown(KeyCode.Alpha4)){
                StartCoroutine(processDial(4));
            }
            if(Input.GetKeyDown(KeyCode.Alpha5)){
                StartCoroutine(processDial(5));
            }
            if(Input.GetKeyDown(KeyCode.Alpha6)){
                StartCoroutine(processDial(6));
            }
            if(Input.GetKeyDown(KeyCode.Alpha7)){
                StartCoroutine(processDial(7));
            }
            if(Input.GetKeyDown(KeyCode.Alpha8)){
                StartCoroutine(processDial(8));
            }
            if(Input.GetKeyDown(KeyCode.Alpha9)){
                StartCoroutine(processDial(9));
            }
            if(Input.GetKeyDown(KeyCode.Alpha0)){
                StartCoroutine(processDial(0));
            }

        }

        
    }
}
