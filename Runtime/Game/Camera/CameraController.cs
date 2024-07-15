/*
 * --------------------------------------||----->>
 * Darklight Interactive Core Plugin
 * Copyright (c) 2024 Darklight Interactive. All rights reserved.
 * ----------------------------------------------------------------- [[ )) 
 * Licensed under the Darklight Interactive Software License Agreement.
 * See LICENSE.md file in the project root for full license information.
 * ---------------------------------------------------------------------------- [[ )) 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * ----------------------------------------------------------------- [[ )) 
 * For questions regarding this software or licensing, please contact:
 * - Email: skysfalling22@gmail.com
 * - Discord: skysfalling
 * =========================================================== }}
 * Major Authors: 
 * Sky Casey
 *
 */

using UnityEngine;
using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;

namespace Darklight.UnityExt.Game.Camera
{
    [ExecuteAlways]
    public class CameraController : CameraRig
    {
        public enum CameraStateKey { DEFAULT, FOLLOW_TARGET, CLOSE_UP }

        #region State Machine ============================================== >>>>
        public StateMachine _stateMachine;

        public class StateMachine : FiniteStateMachine<CameraStateKey>
        {
            public StateMachine(Dictionary<CameraStateKey, FiniteState<CameraStateKey>> possibleStates, CameraStateKey initialState, params object[] args) : base(possibleStates, initialState, args) { }
        }

        /// <summary>
        /// This state is the default state for the camera. 
        /// It does not follow any target and is in a fixed position.
        /// </summary>
        public class CameraState : FiniteState<CameraStateKey>
        {
            private CameraRig _cameraRig;
            private float _offsetFOV;

            /// <param name="args">
            ///     args[0] = CameraRig ( cameraRig )
            ///     args[1] = float ( FOVOffset )
            /// </param>
            public CameraState(StateMachine stateMachine, CameraStateKey stateType, params object[] args) : base(stateMachine, stateType)
            {
                _cameraRig = (CameraRig)args[0];
                _offsetFOV = (float)args[1];
            }

            public override void Enter()
            {
                _cameraRig.SetOffsetFOV(_offsetFOV);
            }

            public override void Exit() { }
            public override void Execute() { }
        }
        #endregion

        public override void Update()
        {
            base.Update();

            if (_stateMachine != null)
                _stateMachine.Step();
        }
    }
}



