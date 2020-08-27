using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Room))]
public class RoomEditor :  Editor{
	private Room room;
	private Color roomRectColor = new Color(0.5f,0.5f,0f,0.1f);
	private Color roomRectOutline = new Color (0.8f,0f,0f,1f);

	private Vector3[] roomRectPositions = new Vector3[4];

	private float bttnSize = 0.075f;
	private float pickSize = 0.3f;

	private bool[] isClicekdPosBttn = new bool[4];

	public void OnSceneGUI()
	{
		room = target as Room;
		if (null == room)
			return;
		Handles.DrawSolidRectangleWithOutline (room.roomInfo.roomRect, roomRectColor, roomRectOutline);

		var roomRect = room.roomInfo.roomRect;
		var roomCenter = new Vector3 (roomRect.center.x, roomRect.center.y, 0);
		roomRectPositions[0] = roomCenter + Vector3.right * roomRect.width * 0.5f;
		roomRectPositions[1] = roomCenter + Vector3.left * roomRect.width * 0.5f;
		roomRectPositions[2] = roomCenter + Vector3.up * roomRect.height * 0.5f;
		roomRectPositions[3] = roomCenter + Vector3.down * roomRect.height * 0.5f;
		Vector3 center = Vector3.zero;
		for (int i = 0; i < 4; i++)
		{
			var pos = roomRectPositions [i];
			if (Handles.Button (pos, Quaternion.identity, bttnSize, pickSize, Handles.DotHandleCap)) {
				isClicekdPosBttn [i] = !isClicekdPosBttn [i];
			}
			if (isClicekdPosBttn [i]) {
				EditorGUI.BeginChangeCheck ();
				roomRectPositions[i] = Handles.PositionHandle (pos, Quaternion.identity);
				if (EditorGUI.EndChangeCheck()) {
					Undo.RecordObject (room, "Change Rect");
				}
			}
			center += roomRectPositions [i];
		}
		center = center / 4;
		//var newWidth = Mathf.Abs (roomRectPositions [0].x - roomRectPositions [1].x);
		//var newHeight = Mathf.Abs (roomRectPositions [2].y - roomRectPositions [3].y);
		//var deltaWidth = newWidth - room.roomInfo.roomRect.width;
		//var deltaHeight = newHeight - room.roomInfo.roomRect.height;
		//room.roomInfo.roomRect.width = newWidth + deltaWidth;
		//room.roomInfo.roomRect.height = newHeight + deltaHeight;
		room.roomInfo.roomRect.center = new Vector2 (center.x, center.y);
	}
}
