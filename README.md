<p align="center">
  <img src="https://github.com/user-attachments/assets/14d35d0c-052c-4bb8-89ae-9d173f1046ea" alt="Fractal Renderer" width="100%">
</p>

<h1>Fractalis</h1>
<p>
  A feature-packed, cross-platform fractal renderer. Featuring distributed video rendering, layer compositing, and high extensibility. All with a clean CLI interface utilizing Spectre.NET.
</p>

<p align="center">
  <img width="800" height="450" alt="Fractalis demo" src="https://github.com/user-attachments/assets/daedca9c-4caf-4905-aa8b-d786259bc806" />
</p>

<h2>Feature Highlights</h2>

<p align="center">
  <img width="400" src="https://github.com/user-attachments/assets/a1ce82e3-c30d-42f6-82e5-791b5d36ff08" />
  <img width="400" src="https://github.com/user-attachments/assets/77a3ab7f-7ee3-4723-8e80-7fff1bf2c819" />
</p>
<p align="center">
  <img width="400" src="https://github.com/user-attachments/assets/62160f8c-b43b-4419-8684-74308abf6755" />
  <img width="400" src="https://github.com/user-attachments/assets/d37e6881-4143-458a-b1eb-7acb3a996926" />
</p>

<h2>Getting Started</h2>

<h3>1. Install GMP and MPFR</h3>

<p>Fractalis uses GMP and MPFR for arbitrary-precision arithmetic. Install them for your platform before running.</p>

<details>
<summary><b>Windows</b></summary>
<br>
GMP and MPFR are bundled in the release zip. No installation needed. If you'd prefer to install them yourself, you can do so via <a href="https://www.msys2.org/">MSYS2</a>.
</details>

<details>
<summary><b>macOS</b></summary>

```bash
brew install gmp mpfr
```
</details>

<details>
<summary><b>Linux (Debian / Ubuntu)</b></summary>

```bash
sudo apt install libgmp-dev libmpfr-dev
```
</details>

<details>
<summary><b>Linux (Fedora / RHEL)</b></summary>

```bash
sudo dnf install gmp-devel mpfr-devel
```
</details>

<h3>2. Basic rendering</h3>

<p>Run <code>fractalis</code> and it will walk you through render configuration via an interactive CLI.</p>

<h3>3. Distributed video rendering</h3>

<p>
  Start <code>fractalis.Server</code> on any machine - the server's IP and port will be displayed on startup.
  Then run <code>fractalis.Client</code> on any machines you want to use as workers, including your own.
  Once you're ready, open <code>fractalis</code>, select the distributed rendering option in the video configuration, enter the server address, and kick off the render.
  Workers can join and leave at any point during the render.
</p>

<h2>Configuration</h2>

<p>
  After going through the configurator, you can export your render settings to a JSON file.
  This gives you a reusable blueprint for that exact setup, which you can load back in via the <code>--config "[PATH_TO_CONFIG]"</code> flag or through the CLI interface itself.
  To tweak a render without starting from scratch, just edit the exported JSON directly.
</p>

<h2>Project Structure</h2>

| Project | Role |
|---|---|
| `fractalis` | CLI entry point |
| `fractalis.Core` | Main library for renderers, fractals, networking, etc. |
| `fractalis.Server` | Orchestrator - server for distributed rendering |
| `fractalis.Client` | Worker client - does the work assigned by the Orchestrator |
| `fractalis.Test` | Test suite |

<h2>License</h2>

<p>GPL-3.0 — see <a href="LICENSE">LICENSE</a> for details.</p>
