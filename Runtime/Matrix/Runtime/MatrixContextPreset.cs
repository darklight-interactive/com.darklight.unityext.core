using Darklight.UnityExt.Behaviour;

namespace Darklight.UnityExt.Matrix
{
    public class MatrixContextPreset : ScriptableData<Context>
    {
        void OnValidate() => data.Validate();
        public override void SetData(Context data)
        {
            data.Validate();
            base.SetData(data);
        }

        public override Context ToData()
        {
            data.Validate();
            return data;
        }
    }
}