"""Non-product placeholder so CodeQL default-setup Python analysis has source.

VirusTotalUploader is a C# WinForms app. This PR deletes fix_tests.py, which
was a local worktree helper and the only Python file. GitHub CodeQL default
setup still runs a python language job from repo settings (master still has
that file). With zero .py files, `codeql database finalize` exits 32 and the
Analyze (python) / CodeQL checks fail.

A CodeQL config YAML cannot drop languages for default setup, and an advanced
workflow would not stop the managed `on: dynamic` python job. This module is
not application code. After merge, uncheck Python in Settings → Advanced
Security → CodeQL default setup, then delete this file.
"""


def placeholder() -> str:
    return "codeql-python-placeholder"
