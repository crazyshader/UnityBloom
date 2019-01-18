// Amplify Bloom - Advanced Bloom Post-Effect for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using UnityEngine.EventSystems;

namespace AmplifyBloom
{
	public class DemoCameraMovement : MonoBehaviour
	{

		private const string X_AXIS_KEYBOARD = "Mouse X";
		private const string Y_AXIS_KEYBOARD = "Mouse Y";

		private const string X_AXIS_GAMEPAD = "Horizontal";
		private const string Y_AXIS_GAMEPAD = "Vertical";

		private bool m_gamePadMode = false;

		public float moveSpeed = 1f;
		public float yawSpeed = 3;
		public float pitchSpeed = 3;

		private float _yaw = 0;
		private float _pitch = 0;

		private Transform _transform;

		void Start()
		{
			_transform = transform;

			_pitch = _transform.localEulerAngles.x;
			_yaw = _transform.localEulerAngles.y;

#if !UNITY_EDITOR && ( UNITY_IOS || UNITY_ANDROID )
			m_gamePadMode = false;
#else
			if ( Input.GetJoystickNames().Length > 0 )
			{
				m_gamePadMode = true;
			}
#endif

		}

		void Update()
		{
#if !UNITY_EDITOR && ( UNITY_IOS || UNITY_ANDROID )

			if ( !EventSystem.current.IsPointerOverGameObject() )
			{
				if ( Input.touchCount > 0 )
				{
					Vector2 deltaPos = Input.GetTouch( 0 ).deltaPosition;
					ChangeYaw( deltaPos.x * yawSpeed );
					ChangePitch( -deltaPos.y * pitchSpeed );
				}
			}

#else
			if ( m_gamePadMode )
			{
				ChangeYaw( Input.GetAxisRaw( X_AXIS_GAMEPAD ) * yawSpeed );
				ChangePitch( -Input.GetAxisRaw( Y_AXIS_GAMEPAD ) * pitchSpeed );
			}
			else
			{
				if ( Input.GetMouseButton( 0 ) && !EventSystem.current.IsPointerOverGameObject() )
				{
					ChangeYaw( Input.GetAxisRaw( X_AXIS_KEYBOARD ) * yawSpeed );
					ChangePitch( -Input.GetAxisRaw( Y_AXIS_KEYBOARD ) * pitchSpeed );
				}
			}
#endif
		}

		void MoveForwards( float delta )
		{
			_transform.position += delta * _transform.forward;
		}

		void Strafe( float delta )
		{
			_transform.position += delta * _transform.right;
		}

		void ChangeYaw( float delta )
		{
			_yaw += delta;
			WrapAngle( ref _yaw );
			_transform.localEulerAngles = new Vector3( _pitch, _yaw, 0 );
		}

		void ChangePitch( float delta )
		{
			_pitch += delta;
			WrapAngle( ref _pitch );
			_transform.localEulerAngles = new Vector3( _pitch, _yaw, 0 );
		}

		public void WrapAngle( ref float angle )
		{
			if ( angle < 0 )
				angle = 360 + angle;

			if ( angle > 360 )
				angle = angle - 360;
		}

		public bool GamePadMode
		{
			get
			{
				return m_gamePadMode;
			}
		}

	}
}
