using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TouchButton : MonoBehaviour, 
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public enum TouchButtonType {
        ATTACK = 0,
        PARRY,
        ROLL,
        JUMP,
        DOWN,
        UP,
        RIGHT,
        LEFT
    };
    public TouchButtonType tbType;
    private Button selfButton;
    private bool buttonPressed = false;
    public GameSystemService systemService;
    // Use this for initialization
    void Start () {
        selfButton = GetComponent<Button>();
        if (null == systemService)
            systemService = GameSystemService.Instance;
        if (tbType == TouchButtonType.ROLL)
            selfButton.onClick.AddListener(OnRollClicked);
	}
	public void OnAttackClicked()
    {
        systemService.playerInputManager.OnAttackKeyDownEvent(KeyCode.A);
    }
    public void OnParryClicked()
    {
        systemService.playerInputManager.OnAttackKeyDownEvent(KeyCode.S);
    }
    public void OnRollClicked()
    {
        systemService.playerInputManager.OnRollingDownEvent(KeyCode.Space);
    }
    public void OnJumpDown()
    {
        systemService.playerInputManager.OnJumpKeyPressedEvent(KeyCode.D);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonPressed = true;
        if (tbType == TouchButtonType.JUMP)
            OnJumpDown();
        else if (tbType == TouchButtonType.ATTACK)
            OnAttackClicked();
        else if (tbType == TouchButtonType.PARRY)
            OnParryClicked();
        else if (tbType == TouchButtonType.DOWN)
        {
            GameSystemService.Instance.playerInputManager.OnAttackKeyDownEvent(KeyCode.DownArrow);
            GameSystemService.Instance.playerInputManager.OnMoveKeyDownEvent(KeyCode.DownArrow);
        }
        else if (tbType == TouchButtonType.UP)
        {
            GameSystemService.Instance.playerInputManager.OnAttackKeyDownEvent(KeyCode.UpArrow);
            GameSystemService.Instance.playerInputManager.OnMoveKeyDownEvent(KeyCode.UpArrow);
        }
        else if (tbType == TouchButtonType.RIGHT)
        {
            GameSystemService.Instance.playerInputManager.OnAttackKeyDownEvent(KeyCode.RightArrow);
            GameSystemService.Instance.playerInputManager.OnMoveKeyDownEvent(KeyCode.RightArrow);
        }
        else if (tbType == TouchButtonType.LEFT)
        {
            GameSystemService.Instance.playerInputManager.OnAttackKeyDownEvent(KeyCode.LeftArrow);
            GameSystemService.Instance.playerInputManager.OnMoveKeyDownEvent(KeyCode.LeftArrow);
        }
    }

    public void Update()
    {
        if (buttonPressed)
        {
            if (tbType == TouchButtonType.JUMP)
                OnJumpDown();
            else if (tbType == TouchButtonType.DOWN)
            {
                GameSystemService.Instance.playerInputManager.OnMoveKeyPressedEvent(KeyCode.DownArrow);
            }
            else if (tbType == TouchButtonType.UP)
            {
                GameSystemService.Instance.playerInputManager.OnMoveKeyPressedEvent(KeyCode.UpArrow);
            }
            else if (tbType == TouchButtonType.RIGHT)
            {
                GameSystemService.Instance.playerInputManager.OnMoveKeyPressedEvent(KeyCode.RightArrow);
            }
            else if (tbType == TouchButtonType.LEFT)
            {
                GameSystemService.Instance.playerInputManager.OnMoveKeyPressedEvent(KeyCode.LeftArrow);
            }
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        buttonPressed = false;
        if (tbType == TouchButtonType.DOWN)
        {
            PlayerInputManager.pressedSitInput = false;
            PlayerInputManager.pressedMoveInput = false;
            GameSystemService.Instance.playerInputManager.OnMoveKeyUpEvent(KeyCode.DownArrow);
        }
        else if (tbType == TouchButtonType.UP)
        {
            GameSystemService.Instance.playerInputManager.OnMoveKeyUpEvent(KeyCode.UpArrow);
        }
        else if (tbType == TouchButtonType.RIGHT)
        {
            GameSystemService.Instance.playerInputManager.OnMoveKeyUpEvent(KeyCode.RightArrow);
            PlayerInputManager.pressedMoveInput = false;
        }
        else if (tbType == TouchButtonType.LEFT)
        {
            GameSystemService.Instance.playerInputManager.OnMoveKeyUpEvent(KeyCode.LeftArrow);
            PlayerInputManager.pressedMoveInput = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonPressed = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonPressed = false;
    }
}
