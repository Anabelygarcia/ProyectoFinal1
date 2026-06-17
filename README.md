//Lotería Mexicana — ProyectoFinal1//

Videojuego de mesa de Lotería Mexicana desarrollado en C# con Windows Forms. Permite jugar en modo local o en red (LAN) contra otros jugadores, con tableros personalizables, distintos modos de victoria, sonidos auténticos de cada carta y un sistema de chat integrado.

--Características--

- 54 cartas tradicionales de la Lotería Mexicana, cada una con su nombre, su "grito" característico y su sonido en formato `.wav`.
- Modo multijugador en red: un jugador puede actuar como anfitrión (host) y el resto se conecta usando su dirección IP, mediante un servidor TCP propio (`ChatServer`).
- Chat en vivo entre los jugadores conectados a la partida.
- Varios modos de victoria, incluyendo:
  - Línea horizontal
  - Línea vertical
  - Diagonal
  - Forma de L
  - Cruz central
  - Marco (bordes)
  - Tabla llena
  - Modo personalizado (el jugador elige las celdas que deben completarse)
- Modo del día**: si no se elige un modo manualmente, el juego asigna automáticamente un patrón de victoria distinto según el día de la semana.
- Editor de tableros personalizado para armar y guardar tus propias tablas de 4x4.
- Selector de tableros guardados para reutilizar tablas creadas previamente.
- Múltiples tableros por jugador (hasta 3 tablas simultáneas por partida).
- Reproducción de audio de cada carta al ser cantada, así como efectos de sonido al barajar y al declarar ganador, mediante la librería NAudio.
- Generación de imágenes/placeholders a color para cada carta cuando no hay una imagen disponible.
- Pantalla de ganador dedicada al finalizar cada ronda.

--¿Cómo ejecutar el proyecto?--

-Requisitos

- [Visual Studio 2022](https://visualstudio.microsoft.com/) (o posterior) con la carga de trabajo Desarrollo de escritorio de .NET
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Windows (el proyecto usa Windows Forms, por lo que solo corre en Windows)

-Pasos

1. Clona el repositorio:
      bash
   git clone https://github.com/Anabelygarcia/ProyectoFinal1.git
   
2. Abre "ProyectoFinal1.sln" con Visual Studio.
3. Restaura los paquetes NuGet (Visual Studio lo hace automáticamente, o ejecuta `dotnet restore`).
4. Compila y ejecuta el proyecto (`F5` en Visual Studio, o):
      bash
   dotnet run --project ProyectoFinal1/ProyectoFinal1.csproj
   

--¿Cómo jugar?--

1. Al iniciar la aplicación se abre el Menú Principal, donde debes escribir tu nombre de jugador.
2. Elige si quieres ser Host (anfitrión de la partida) o **unirte** introduciendo la IP del host.
   - Si eres host, comparte la IP que se muestra en pantalla con tus amigos para que se conecten (puerto 9876).
3. Selecciona el modo de victoria deseado (o deja el "modo del día") y la cantidad de tableros que quieres jugar.
4. Personaliza tu tablero desde el editor o usa uno generado/guardado previamente.
5. Una vez listos todos los jugadores, el host inicia la partida y comienza a "cantar" las cartas.
6. Marca en tu tablero las cartas que van saliendo y declara "¡Lotería!" cuando completes el patrón del modo elegido.

--Autoría--

Proyecto final desarrollado como parte de un curso de programación en C#.

--Licencia--

Este proyecto se distribuye con fines educativos. Puedes adaptarlo y reutilizarlo libremente citando la fuente original.
