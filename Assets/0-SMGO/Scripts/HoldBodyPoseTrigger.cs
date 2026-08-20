using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

public class HoldBodyPoseTrigger : MonoBehaviour
{
    [SerializeField, Interface(typeof(IActiveState))] private UnityEngine.Object bodyPoseActiveState;
    [SerializeField] private float holdTime = 3f;
    [SerializeField] private Transform progressBar;

    [SerializeField] private AudioSource tickingAudio;

    [SerializeField] private UnityEvent onPoseActivated;
    
    [SerializeField] private UnityEvent onHoldComplete;

    private IActiveState activeState;
    private float timer;
    private bool completed;

    private void Awake()
    {
        activeState = bodyPoseActiveState as IActiveState;
        progressBar.localScale = new Vector3(0f, 1f, 1f);
    }

    private void Update()
    {
        if (activeState == null)
            return;

        if (activeState.Active)
        {
            if (timer == 0f)
            {
                onPoseActivated.Invoke();
                tickingAudio.Play();
            }


            if (!completed)
            {
                timer += Time.deltaTime;

                float progress = Mathf.Clamp01(timer / holdTime);
                progressBar.localScale = new Vector3(progress, 1f, 1f);

                if (timer >= holdTime)
                {
                    completed = true;
                    tickingAudio.Stop();
                    onHoldComplete.Invoke();
                }
            }
        }
        else
        {
            timer = 0f;
            completed = false;
            progressBar.localScale = new Vector3(0f, 1f, 1f);
            tickingAudio.Stop();
            
        }
    }
}