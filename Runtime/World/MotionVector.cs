/* ======================================================================= ]]
 * Copyright (c) 2024 Darklight Interactive. All rights reserved.
 * Licensed under the Darklight Interactive Software License Agreement.
 * See LICENSE.md file in the project root for full license information.
 * ------------------------------------------------------------------ >>
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * ------------------------------------------------------------------ >>
 * For questions regarding this software or licensing, please contact:
 * Email: skysfalling22@gmail.com
 * Discord: skysfalling
 * ======================================================================= ]]
 * DESCRIPTION:
    This script defines a Motion Vector in 3D space.
    Mainly used for movement physics calculations.
 * ------------------------------------------------------------------ >>
 * MAJOR AUTHORS:
 * Sky Casey
 * ======================================================================= ]]
 */

using UnityEngine;

namespace Darklight.World
{
    /// <summary>
    /// A 3D Vector Utility Class that can be used to store and manipulate 3D vectors.
    /// </summary>
    [System.Serializable]
    public class MotionVector
    {
        [SerializeField]
        Vector3 _vector = Vector3.zero;

        public MotionVector()
        {
            Horizontal = Vector2.zero;
            Vertical = 0f;
        }

        public MotionVector(Vector2 horizontal, float vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
        }

        public static MotionVector Zero => new MotionVector(Vector2.zero, 0f);

        /// <summary>
        /// The horizontal component of the vector. This is a 2D vector with an x and z component.
        /// </summary>
        public Vector2 Horizontal
        {
            get => new Vector2(_vector.x, _vector.z);
            set
            {
                _vector.x = value.x;
                _vector.z = value.y;
            }
        }

        /// <summary>
        /// The horizontal component of the vector as a 3D vector.
        /// </summary>
        public Vector3 HorizontalVec3
        {
            get => new Vector3(Horizontal.x, 0f, Horizontal.y);
            set { Horizontal = new Vector2(value.x, value.z); }
        }

        /// <summary>
        /// The vertical component of the vector. This is a float value of the y component of the vector.
        /// </summary>
        public float Vertical
        {
            get => _vector.y;
            set => _vector.y = value;
        }

        /// <summary>
        /// The vertical component of the vector as a 3D vector.
        /// </summary>
        public Vector3 VerticalVec3
        {
            get => new Vector3(0f, Vertical, 0f);
            set => Vertical = value.y;
        }

        /// <summary>
        /// The combined vector of the horizontal and vertical components.
        /// </summary>
        public Vector3 Combined
        {
            get => new Vector3(Horizontal.x, Vertical, Horizontal.y);
            set
            {
                Horizontal = new Vector2(value.x, value.z);
                Vertical = value.y;
            }
        }

        /// <summary>
        /// The normalized vector of the combined vector.
        /// </summary>
        public Vector3 Normalized => Combined.normalized;

        /// <summary>
        /// The magnitude of the combined vector.
        /// </summary>
        public float Magnitude => Combined.magnitude;

        /// <summary>
        /// Sets the vector to the given 3D vector.
        /// </summary>
        /// <param name="vector">The vector to set the current vector to.</param>
        public void Set(Vector3 vector)
        {
            Horizontal = new Vector2(vector.x, vector.z);
            Vertical = vector.y;
        }

        /// <summary>
        /// Sets the vector to the given horizontal and vertical components.
        /// </summary>
        /// <param name="horizontal">The horizontal component of the vector.</param>
        /// <param name="vertical">The vertical component of the vector.</param>
        public void Set(Vector2 horizontal, float vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
        }

        /// <summary>
        /// Sets the vector to the given motion vector.
        /// </summary>
        /// <param name="motionVector">The motion vector to set the current vector to.</param>
        public void Set(MotionVector motionVector)
        {
            Horizontal = motionVector.Horizontal;
            Vertical = motionVector.Vertical;
        }

        /// <summary>
        /// Sets the vector to zero.
        /// </summary>
        public void SetZero()
        {
            Horizontal = Vector2.zero;
            Vertical = 0f;
        }

        /// <summary>
        /// Sets the vector to one.
        /// </summary>
        public void SetOne()
        {
            Horizontal = Vector2.one;
            Vertical = 1f;
        }

        /// <summary>
        /// Clamps the vector to the given horizontal and vertical clamps.
        /// </summary>
        /// <param name="horzClamp">The horizontal clamp.</param>
        /// <param name="vertClamp">The vertical clamp.</param>
        public void Clamp(float horzClamp, float vertClamp)
        {
            Horizontal = new Vector2(
                Mathf.Clamp(Horizontal.x, -horzClamp, horzClamp),
                Mathf.Clamp(Horizontal.y, -horzClamp, horzClamp)
            );
            Vertical = Mathf.Clamp(Vertical, -vertClamp, vertClamp);
        }

        /// <summary>
        /// Resets the vector to zero.
        /// </summary>
        public void Reset()
        {
            Horizontal = Vector2.zero;
            Vertical = 0f;
        }

        /// <summary>
        /// Prints the vector to the console.
        /// </summary>
        public string Print()
        {
            return $"-> MOTIONVECTOR: Horizontal: {Horizontal}, Vertical: {Vertical}";
        }

        /// <summary>
        /// Lerps between two motion vectors.
        /// </summary>
        /// <param name="a">The first motion vector.</param>
        /// <param name="b">The second motion vector.</param>
        /// <param name="t">The interpolation factor.</param>
        public static MotionVector Lerp(MotionVector a, MotionVector b, float t)
        {
            return new MotionVector(
                Vector2.Lerp(a.Horizontal, b.Horizontal, t),
                Mathf.Lerp(a.Vertical, b.Vertical, t)
            );
        }
    }
}
