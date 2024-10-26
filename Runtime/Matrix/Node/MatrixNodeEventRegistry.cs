
namespace Darklight.UnityExt.Matrix
{
    partial class MatrixNode
    {
        public static class EventRegistry
        {
            public delegate bool VisitCellEvent(MatrixNode cell);
            public delegate bool VisitCellComponentEvent(MatrixNode cell, ComponentTypeKey componentTypeKey);

            // ======== [[ BASE VISITOR FUNCTIONS ]] ================================== >>>>
            /// <summary>
            /// Register the component to the cell.
            /// </summary>
            public static VisitCellComponentEvent BaseRegisterFunc => (MatrixNode cell, ComponentTypeKey type) =>
            {
                Component component = cell.ComponentReg.RegisterComponent(type);
                return component != null;
            };

            /// <summary>
            /// The Default Initialization function to call when visiting the cell.
            /// </summary>
            public static VisitCellComponentEvent BaseInitFunc => (MatrixNode cell, ComponentTypeKey type) =>
            {
                Component component = cell.ComponentReg.GetComponent(type);
                if (component == null) return false;

                // < INITIALIZATION >
                if (!component.Initialized)
                {
                    component.OnInitialize(cell);
                    return true;
                }

                return false;
            };

            /// <summary>
            /// The Default Update function to call when visiting the cell.
            /// </summary>
            public static VisitCellComponentEvent BaseUpdateFunc => (MatrixNode cell, ComponentTypeKey type) =>
            {
                Component component = cell.ComponentReg.GetComponent(type);
                if (component == null) return false;

                // < INITIALIZATION >
                if (!component.Initialized)
                {
                    component.OnInitialize(cell);
                    return true;
                }

                // < UPDATE >
                component.OnUpdate();
                return true;
            };

            public static VisitCellComponentEvent BaseGizmosFunc => (MatrixNode cell, ComponentTypeKey type) =>
            {
                Component component = cell.ComponentReg.GetComponent(type);
                if (component == null) return false;

                component.DrawGizmos();
                return true;
            };

            public static VisitCellComponentEvent BaseSelectedGizmosFunc => (MatrixNode cell, ComponentTypeKey type) =>
            {
                Component component = cell.ComponentReg.GetComponent(type);
                if (component == null) return false;

                component.DrawSelectedGizmos();
                return true;
            };

            public static VisitCellComponentEvent BaseEditorGizmosFunc => (MatrixNode cell, ComponentTypeKey type) =>
            {
                Component component = cell.ComponentReg.GetComponent(type);
                if (component == null) return false;

                component.DrawEditorGizmos();
                return true;
            };

        }
    }
}