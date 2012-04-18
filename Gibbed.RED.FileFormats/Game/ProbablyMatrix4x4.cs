/* Copyright (c) 2012 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

namespace Gibbed.RED.FileFormats.Game
{
    // ReSharper disable InconsistentNaming
    public class ProbablyMatrix4x4 : IFileObject
        // ReSharper restore InconsistentNaming
    {
        public ProbablyMatrix A;
        public ProbablyMatrix B;
        public ProbablyMatrix C;
        public ProbablyMatrix D;

        public void Serialize(IFileStream stream)
        {
            stream.SerializeObject(ref this.A);
            stream.SerializeObject(ref this.B);
            stream.SerializeObject(ref this.C);
            stream.SerializeObject(ref this.D);
        }

        public class ProbablyMatrix : IFileObject
        {
            public float X;
            public float Y;
            public float Z;
            public float W;

            public void Serialize(IFileStream stream)
            {
                stream.SerializeValue(ref this.X);
                stream.SerializeValue(ref this.Y);
                stream.SerializeValue(ref this.Z);
                stream.SerializeValue(ref this.W);
            }
        }
    }
}
