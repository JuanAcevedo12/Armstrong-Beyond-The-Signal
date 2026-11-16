using UnityEngine;using UnityEngine.EventSystems;
namespace Caronte {
public enum TouchSurfaceKind{Move,Look,Preview,Up,Down}
public sealed class MobileTouchSurface:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IDragHandler,IInitializePotentialDragHandler {
 public TouchSurfaceKind kind;public RectTransform knob;public CarontePrototype app;int pointer=int.MinValue;RectTransform area;
 void Awake(){area=GetComponent<RectTransform>();}
 public void OnInitializePotentialDrag(PointerEventData e){e.useDragThreshold=false;}
 public void OnPointerDown(PointerEventData e){if(!MobileRuntime.Active||pointer!=int.MinValue)return;pointer=e.pointerId;if(kind==TouchSurfaceKind.Move)Move(e);if(kind==TouchSurfaceKind.Up)MobileRuntime.UpHeld=true;if(kind==TouchSurfaceKind.Down)MobileRuntime.DownHeld=true;}
 public void OnDrag(PointerEventData e){if(e.pointerId!=pointer)return;if(kind==TouchSurfaceKind.Move)Move(e);else if(kind==TouchSurfaceKind.Look)MobileRuntime.LookDelta+=e.delta*(1080f/Mathf.Max(1,Screen.height));else if(kind==TouchSurfaceKind.Preview&&app)app.RotatePreview(e.delta*(1080f/Mathf.Max(1,Screen.height)));}
 void Move(PointerEventData e){Vector2 local;RectTransformUtility.ScreenPointToLocalPointInRectangle(area,e.position,e.pressEventCamera,out local);local-=area.rect.center;float radius=Mathf.Min(area.rect.width,area.rect.height)*.36f;Vector2 value=Vector2.ClampMagnitude(local/Mathf.Max(1,radius),1);MobileRuntime.Move=value.magnitude<.12f?Vector2.zero:value;if(knob)knob.anchoredPosition=value*radius;}
 public void OnPointerUp(PointerEventData e){if(pointer==e.pointerId)Release();}
 void Release(){pointer=int.MinValue;if(kind==TouchSurfaceKind.Move){MobileRuntime.Move=Vector2.zero;if(knob)knob.anchoredPosition=Vector2.zero;}if(kind==TouchSurfaceKind.Up)MobileRuntime.UpHeld=false;if(kind==TouchSurfaceKind.Down)MobileRuntime.DownHeld=false;if(kind==TouchSurfaceKind.Look)MobileRuntime.LookDelta=Vector2.zero;}
 void OnDisable(){Release();}
}
}