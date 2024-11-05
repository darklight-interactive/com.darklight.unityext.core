using UnityEditor;
using UnityEngine;

#if UNITY_2019_3_OR_NEWER
using UnityEditor.Compilation;
#elif UNITY_2017_1_OR_NEWER
using System.Reflection;
#endif

using System.Linq;

public class CompilationWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private bool showAssemblyInfo = false;
    private bool autoCompileEnabled = true;
    private double lastCompileTime = 0;

    // Cache assemblies data
    private Assembly[] cachedAssemblies;
    private float nextAssemblyUpdateTime;
    private const float ASSEMBLY_UPDATE_INTERVAL = 1f; // Update assembly info every second

    [MenuItem("Tools/Darklight/CompilationWindow")]
    private static void ShowWindow()
    {
        var window = GetWindow<CompilationWindow>();
        window.titleContent = new GUIContent("Compiler Window");
        window.minSize = new Vector2(300, 200);
        window.Show();
    }

    private void OnEnable()
    {
#if UNITY_2019_3_OR_NEWER
        CompilationPipeline.compilationStarted += OnCompilationStarted;
        CompilationPipeline.compilationFinished += OnCompilationFinished;
        UpdateAssemblyCache();
#endif
    }

    private void UpdateAssemblyCache()
    {
#if UNITY_2019_3_OR_NEWER
        if (Time.realtimeSinceStartup >= nextAssemblyUpdateTime)
        {
            cachedAssemblies = CompilationPipeline.GetAssemblies();
            nextAssemblyUpdateTime = Time.realtimeSinceStartup + ASSEMBLY_UPDATE_INTERVAL;
        }
#endif
    }

    private void OnDisable()
    {
#if UNITY_2019_3_OR_NEWER
        CompilationPipeline.compilationStarted -= OnCompilationStarted;
        CompilationPipeline.compilationFinished -= OnCompilationFinished;
#endif
    }

    private void OnGUI()
    {
        using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
        {
            scrollPosition = scrollView.scrollPosition;

            EditorGUILayout.Space(10);

            // Compilation Controls
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Compilation Controls", EditorStyles.boldLabel);
                
                if (GUILayout.Button("Force Recompile Scripts"))
                {
                    ForceRecompile();
                }

                autoCompileEnabled = EditorGUILayout.Toggle("Auto-Compile Enabled", autoCompileEnabled);
                EditorPrefs.SetBool("kAutoRefresh", autoCompileEnabled);

                if (lastCompileTime > 0)
                {
                    EditorGUILayout.LabelField($"Last Compile Time: {FormatCompileTime(lastCompileTime)}");
                }
            }

            EditorGUILayout.Space(10);

            // Assembly Information
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                showAssemblyInfo = EditorGUILayout.Foldout(showAssemblyInfo, "Assembly Information", true);
                if (showAssemblyInfo)
                {
                    // Only update assembly cache when needed
                    if (cachedAssemblies == null)
                    {
                        UpdateAssemblyCache();
                    }

                    if (cachedAssemblies == null || cachedAssemblies.Length == 0) return;
                    var sortedAssemblies = cachedAssemblies.OrderByDescending(a => a.allReferences.Length);
                    
                    foreach (var assembly in sortedAssemblies)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField($"Assembly: {assembly.name}");
                        }
                        EditorGUI.indentLevel++;
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField($"Output: {assembly.outputPath}");
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField($"References: {assembly.allReferences.Length}");
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }

            EditorGUILayout.Space(10);

            // Compilation Status
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Compilation Status", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Is Compiling: {EditorApplication.isCompiling}");
                EditorGUILayout.LabelField($"Is Playing: {EditorApplication.isPlaying}");
                EditorGUILayout.LabelField($"Is Paused: {EditorApplication.isPaused}");
            }
        }
    }

    private void ForceRecompile()
    {
#if UNITY_2019_3_OR_NEWER
        CompilationPipeline.RequestScriptCompilation();
#elif UNITY_2017_1_OR_NEWER
        var editorAssembly = Assembly.GetAssembly(typeof(Editor));
        var editorCompilationInterfaceType = editorAssembly.GetType(
            "UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface"
        );
        var dirtyAllScriptsMethod = editorCompilationInterfaceType.GetMethod(
            "DirtyAllScripts",
            BindingFlags.Static | BindingFlags.Public
        );
        dirtyAllScriptsMethod.Invoke(editorCompilationInterfaceType, null);
#endif
    }

    private void OnCompilationStarted(object obj)
    {
        lastCompileTime = EditorApplication.timeSinceStartup;
    }

    private void OnCompilationFinished(object obj)
    {
        lastCompileTime = EditorApplication.timeSinceStartup - lastCompileTime;
        Repaint();
    }

    private string FormatCompileTime(double seconds)
    {
        int minutes = (int)(seconds / 60);
        seconds = seconds % 60;
        int milliseconds = (int)((seconds % 1) * 1000);
        int wholeSeconds = (int)seconds;

        if (minutes > 0)
            return $"{minutes}m {wholeSeconds}s {milliseconds}ms";
        if (wholeSeconds > 0)
            return $"{wholeSeconds}s {milliseconds}ms";
        return $"{milliseconds}ms";
    }
}
