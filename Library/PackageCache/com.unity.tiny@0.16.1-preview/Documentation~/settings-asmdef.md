# Assembly Definition Settings

Each DOTS Mode project has an accompanying Assembly Definition file. This is created for you by default when you create a new DOTS Mode project.

Below you can see the files that are created for you by default when you create a new DOTS Mode project. The project file itself is represented by the grey file box icon, and the Assembly Definition file is represented by a jigsaw piece icon.

![The asset files for a default new DOTS Mode project showing the .asmdef file (the jigsaw-piece icon) and the .project file (the grey file box icon)](images/new-project-and-asmdef-files.png)

*The asset files for a default new DOTS Mode project showing the .asmdef file (the jigsaw-piece icon) and the .project file (the grey file box icon)*

The assembly definition file allows you to adjust your project's settings and specify which modules should be included in your project. You can use these settings to reduce the size of your built project by removing assemblies that your project does not use.

Assembly Definition files are not specific to DOTS Mode in Unity, therefore you can find [the documentation for Assembly Definition files in the main Unity manual](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html).

![The inspector for the Assembly Definition file.](images/assembly-definition-inspector.png)

*The inspector for the Assembly Definition file.*

**Note:** You must make sure that **Use GUIDs** is *not* enabled. Builds from DOTS Mode will fail if this setting is enabled.

**Note:** If you are using a component on an Entity in your Scene which does not have the correct Assembly Definition Reference added, the Inspector will show a warning when that Entity is selected, along with a button that takes you to the Assembly Definition file, as shown below:

![The Inspector displaying a warning about a missing Assembly.](images/assembly-missing.png)

*The Inspector displaying a warning about a missing Assembly.*

