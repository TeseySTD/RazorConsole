// Copyright (c) RazorConsole. All rights reserved.

namespace RazorConsole.Testing.Pty;

/// <summary>
/// The <c>/bin/bash</c> command line and environment that start a program on macOS (see <see cref="UnixPtyConnection"/>).
/// </summary>
/// <remarks>
/// <para>
/// bash gets the caller's environment, minus the entries bash 3.2 itself acts on (<see cref="ActedOn"/>). argv is
/// readable by every user (<c>ps</c>) while the environment is readable only by its owner, so no other entry may
/// appear in argv. bash then execs <c>/usr/bin/env</c>, which puts the removed entries back, undoes bash's own changes
/// to the exported environment (<see cref="Rewritten"/>: the caller's value, or <c>-u</c> where the caller had none)
/// and execs the program. Only the values of those names reach argv. Apple env(1) (shell_cmds <c>env/env.c</c>) unsets
/// every <c>-u</c> name, stops at <c>--</c>, assigns each following argument containing <c>=</c>, then execs the rest.
/// </para>
/// <para>
/// bash re-exports every other imported variable unchanged: <c>initialize_shell_variables</c> (variables.c) binds
/// each entry under its name, whatever the name, caches the original <c>name=value</c> string
/// (<c>CACHE_IMPORTSTR</c>), and <c>make_env_array_from_var_list</c> exports that string. Locale variables are read
/// (<c>sv_locale</c>) but neither change the script nor the exported values. <c>CDPATH</c> is not consulted because
/// the directory is absolute.
/// </para>
/// </remarks>
internal static class BashLaunch
{
    public const string Bash = "/bin/bash";

    // $0 is the slave path; env(1)'s options and assignments, the program and its arguments follow.
    // 126: the terminal could not be attached or the directory changed.
    private const string AttachScript = "exec 0<>\"$0\" || exit 126; exec /usr/bin/env \"$@\"";
    private const string AttachAndChdirScript =
        "exec 0<>\"$0\" || exit 126; cd -P -- \"$1\" || exit 126; shift; exec /usr/bin/env \"$@\"";

    // Imported functions: bash 3.2 (Apple's patched variables.c) parses and runs a definition for an entry named
    // BASH_FUNC_<name>%% whose value starts with "() {", and exports it reformatted. Every such value is withheld.
    private const string FunctionPrefix = "() {";

    /// <summary>
    /// Gets the variables that change what bash 3.2 runs or how. They are withheld from bash and given to the program
    /// through env(1)'s argv.
    /// </summary>
    /// <remarks>
    /// <c>BASH_ENV</c>: sourced by a non-interactive shell (shell.c <c>run_startup_files</c>). <c>ENV</c>: sourced by an
    /// interactive shell or one running as sh (shell.c), withheld for safety. <c>SHELLOPTS</c>: parsed into
    /// <c>set -o</c> options (<c>initialize_shell_options</c>, builtins/set.def), e.g. <c>xtrace</c> or
    /// <c>allexport</c>. <c>POSIXLY_CORRECT</c> / <c>POSIX_PEDANTIC</c>: turn on posix mode (variables.c,
    /// <c>sv_strict_posix</c>). <c>BASHOPTS</c> does not exist in bash 3.2 (added in 4.1).
    /// </remarks>
    internal static readonly string[] ActedOn = ["BASH_ENV", "ENV", "SHELLOPTS", "POSIXLY_CORRECT", "POSIX_PEDANTIC"];

    /// <summary>
    /// Gets the variables whose exported value bash 3.2 changes, adds or drops before the program starts. env(1)
    /// restores the caller's value or unsets them.
    /// </summary>
    /// <remarks>
    /// variables.c <c>initialize_shell_variables</c>: <c>set_pwd</c> rebinds and exports <c>PWD</c> unless the imported
    /// value names the current directory, and rebinds <c>OLDPWD</c> to an invisible, valueless variable;
    /// <c>IFS</c>, <c>OPTIND</c>, <c>OPTERR</c>, <c>BASH</c>, <c>BASH_VERSION</c> and <c>BASH_EXECUTION_STRING</c> are
    /// rebound (an imported one stays exported with the new value); <c>adjust_shell_level</c> exports <c>SHLVL</c>
    /// + 1 (builtins/exec.def subtracts 1 again before the exec, or exports 0 when the caller had none);
    /// <c>set_ppid</c> unexports <c>PPID</c>; <c>make_vers_array</c> replaces <c>BASH_VERSINFO</c> with an array,
    /// which is never exported. <c>initialize_dynamic_variables</c> rebinds <c>BASH_COMMAND</c>,
    /// <c>BASH_SUBSHELL</c>, <c>RANDOM</c>, <c>LINENO</c>, <c>HISTCMD</c> and <c>COMP_WORDBREAKS</c> without a value.
    /// execute_cmd.c <c>bind_lastarg</c> unexports <c>_</c>. builtins/cd.def binds and exports <c>PWD</c> and
    /// <c>OLDPWD</c>. Variables bash only creates when missing (<c>set_if_not</c>: <c>PATH</c>, <c>TERM</c>, <c>PS4</c>,
    /// <c>HOSTTYPE</c>, ...; <c>SHELL</c>, <c>UID</c>, <c>EUID</c>, <c>SECONDS</c>, <c>GROUPS</c>, ...) are not exported
    /// unless imported, and then keep the imported value.
    /// </remarks>
    internal static readonly string[] Rewritten =
    [
        "PWD", "OLDPWD", "_", "IFS", "SHLVL", "PPID", "OPTIND", "OPTERR", "BASH", "BASH_VERSION", "BASH_VERSINFO",
        "BASH_EXECUTION_STRING", "BASH_COMMAND", "BASH_SUBSHELL", "RANDOM", "LINENO", "HISTCMD", "COMP_WORDBREAKS",
    ];

    /// <summary>
    /// Builds bash's argv, up to the program path (the program's arguments follow it), and bash's environment.
    /// </summary>
    /// <param name="program">The resolved program path; it cannot contain <c>=</c>, or env(1) would take it for an assignment.</param>
    /// <param name="slavePath">The PTY slave path.</param>
    /// <param name="workingDirectory">The directory bash changes to, or <see langword="null"/> to stay.</param>
    /// <param name="environment">The program's environment.</param>
    public static (string[] Arguments, string[] Environment) Build(
        string program, string slavePath, string? workingDirectory, IReadOnlyDictionary<string, string> environment)
    {
        var argv = new List<string> { Bash, "-c", workingDirectory is null ? AttachScript : AttachAndChdirScript, slavePath };
        if (workingDirectory is not null)
        {
            argv.Add(Path.GetFullPath(workingDirectory));
        }

        var envp = new List<string>(environment.Count);
        var assignments = new List<string>();
        foreach (var (name, value) in environment)
        {
            var withheld = ActedOn.Contains(name, StringComparer.Ordinal) || value.StartsWith(FunctionPrefix, StringComparison.Ordinal);
            if (!withheld)
            {
                envp.Add(name + "=" + value);
            }

            if (withheld || Rewritten.Contains(name, StringComparer.Ordinal))
            {
                assignments.Add(name + "=" + value);
            }
        }

        foreach (var name in Rewritten)
        {
            if (!environment.ContainsKey(name))
            {
                argv.AddRange(["-u", name]);
            }
        }

        argv.Add("--");
        argv.AddRange(assignments);
        argv.Add(program);
        return (argv.ToArray(), envp.ToArray());
    }
}
