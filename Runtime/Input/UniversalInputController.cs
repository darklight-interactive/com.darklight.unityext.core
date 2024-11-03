using System.Collections;
using Darklight.UnityExt.Editor;
using UnityEngine;

namespace Darklight.UnityExt.Input
{
	public abstract class UniversalInputController : MonoBehaviour
	{
		[Header("Input")]
		[SerializeField, ShowOnly] Vector2 _moveInput;
		[SerializeField, ShowOnly] bool _moveInputStarted;
		[SerializeField, ShowOnly] bool _moveInputCanceled;

		public Vector2 MoveInput => _moveInput;
		public bool IsMoving => MoveInput.magnitude > 0;

		protected virtual void Awake()
		{
			EnableInputs();
		}

		#region < NONPUBLIC_METHODS > [[ HANDLE INPUTS ]] ================================================================ 
		protected virtual void EnableInputs()
		{
			UniversalInputManager.OnMoveInputStarted += HandleOnMoveInputStarted;
			UniversalInputManager.OnMoveInput += HandleOnMoveInput;
			UniversalInputManager.OnMoveInputCanceled += HandleOnMoveInputCanceled;
		}

		protected virtual void DisableInputs()
		{
			UniversalInputManager.OnMoveInputStarted -= HandleOnMoveInputStarted;
			UniversalInputManager.OnMoveInput -= HandleOnMoveInput;
			UniversalInputManager.OnMoveInputCanceled -= HandleOnMoveInputCanceled;
		}

		protected virtual void HandleOnMoveInputStarted(Vector2 moveInput)
		{
			_moveInput = moveInput;
			_moveInputStarted = true;
		}

		protected virtual void HandleOnMoveInput(Vector2 moveInput)
		{
			_moveInput = moveInput;
		}

		protected virtual void HandleOnMoveInputCanceled()
		{
			_moveInput = Vector2.zero;
			_moveInputStarted = false;
			_moveInputCanceled = true;
			StartCoroutine(MoveInputCanceled());
		}

		IEnumerator MoveInputCanceled()
		{
			yield return new WaitForEndOfFrame();
			_moveInputCanceled = true;

			yield return new WaitForEndOfFrame();
			_moveInputCanceled = false;
		}

		#endregion
	}
}