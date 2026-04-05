using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour
{
    private string baseUrl = "http://127.0.0.1:8080";

    [Header("UI - Autenticación")]
    public TMP_InputField inputUsername;
    public TMP_InputField inputPassword;
    public TMP_InputField inputOldPassword; // NUEVO: Requerido por tu ruta /editarPassword
    public TMP_InputField inputNuevaPassword; 
    public Button botonLogin;
    public Button botonRegistro;
    public Button botonEditarPassword; // Renombrado para coincidir con tu backend

    [Header("UI - Funciones Laborales")]
    public Button botonHorarios; 
    public Button botonNomina; // Modificado para llamar a /datosNomina
    public TextMeshProUGUI textoMensaje;

    // Clases serializables para enviar datos al servidor
    [System.Serializable]
    public class DatosUsuario { public string username; public string password; }

    [System.Serializable]
    public class DatosEdicion { public string username; public string oldPassword; public string newPassword; }

    [System.Serializable]
    public class DatosGenericos { public string username; }

    // Clase para recibir datos del servidor
    [System.Serializable]
    public class RespuestaServidor { 
        public bool success; 
        public string mensaje; 
        public string Title; // NUEVO: Para capturar el "Title" que envían /datosNomina, /datosSalario, etc.
    }

    void Start()
    {
        textoMensaje.text = "";

        // Enlazamos todos los botones a sus funciones
        if (botonLogin != null) botonLogin.onClick.AddListener(IntentarLogin);
        if (botonRegistro != null) botonRegistro.onClick.AddListener(IntentarRegistro);
        if (botonEditarPassword != null) botonEditarPassword.onClick.AddListener(EditarPassword);
        if (botonHorarios != null) botonHorarios.onClick.AddListener(ConsultarHorarios);
        if (botonNomina != null) botonNomina.onClick.AddListener(ConsultarNomina);
    }

    // 1. RUTA LOGIN Y REGISTRO (POST)
    public void IntentarLogin() => StartCoroutine(EnviarLogin("/inicioSesion", inputUsername.text, inputPassword.text, true));
    public void IntentarRegistro() => StartCoroutine(EnviarLogin("/registro", inputUsername.text, inputPassword.text, false));

    // 6. RUTA EDICIÓN DE CONTRASEÑA (POST)
    public void EditarPassword() => StartCoroutine(EnviarEdicion("/editarPassword", inputUsername.text, inputOldPassword.text, inputNuevaPassword.text));

    // 3. RUTA DATOS HORARIOS (POST) - Modificado para usar POST según tu index.js
    public void ConsultarHorarios()
    {
        MostrarMensaje("Cargando horarios...", Color.white);
        string json = JsonUtility.ToJson(new DatosGenericos { username = inputUsername.text });
        StartCoroutine(PostRequest("/datosHorarios", json));
    }

    // 2. RUTA DATOS NÓMINA (POST) - Reemplaza al antiguo FicharJornada para probar tus rutas
    public void ConsultarNomina()
    {
        MostrarMensaje("Cargando nómina...", Color.white);
        string json = JsonUtility.ToJson(new DatosGenericos { username = inputUsername.text });
        StartCoroutine(PostRequest("/datosNomina", json));
    }

    // ================= MÉTODOS DE ENVÍO =================

    IEnumerator EnviarLogin(string ruta, string user, string pass, bool esLogin)
    {
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass)) {
            MostrarMensaje("Faltan datos", Color.yellow); yield break;
        }
        string json = JsonUtility.ToJson(new DatosUsuario { username = user, password = pass });
        yield return PostRequest(ruta, json, esLogin);
    }

    IEnumerator EnviarEdicion(string ruta, string user, string oldPass, string newPass)
    {
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(oldPass) || string.IsNullOrEmpty(newPass)) {
            MostrarMensaje("Faltan datos", Color.yellow); yield break;
        }
        string json = JsonUtility.ToJson(new DatosEdicion { username = user, oldPassword = oldPass, newPassword = newPass });
        yield return PostRequest(ruta, json, false);
    }

    // Eliminamos GetRequest ya que todas tus rutas activas son POST
    IEnumerator PostRequest(string ruta, string json, bool cargarEscena = false)
    {
        using (UnityWebRequest request = new UnityWebRequest(baseUrl + ruta, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            ProcesarRespuesta(request, cargarEscena);
        }
    }

    void ProcesarRespuesta(UnityWebRequest request, bool cargarEscena)
    {
        RespuestaServidor resp = JsonUtility.FromJson<RespuestaServidor>(request.downloadHandler.text);
        
        if (request.result != UnityWebRequest.Result.Success) {
            string errorMsg = (resp != null && !string.IsNullOrEmpty(resp.mensaje)) ? resp.mensaje : "Error de conexión o credenciales";
            MostrarMensaje(errorMsg, Color.red);
        } else {
            // Evaluamos si el servidor devolvió 'mensaje' o devolvió 'Title'
            string exitoMsg = "Operación exitosa";
            if (resp != null) {
                if (!string.IsNullOrEmpty(resp.mensaje)) exitoMsg = resp.mensaje;
                else if (!string.IsNullOrEmpty(resp.Title)) exitoMsg = resp.Title;
            }

            MostrarMensaje(exitoMsg, Color.green);
            if (cargarEscena && resp != null && resp.success) StartCoroutine(CambiarEscena());
        }
    }

    IEnumerator CambiarEscena() { yield return new WaitForSeconds(1.5f); SceneManager.LoadScene("Home"); }
    
    void MostrarMensaje(string msg, Color col) { 
        if(textoMensaje != null) {
            textoMensaje.text = msg; 
            textoMensaje.color = col; 
        }
    }
}

/*
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour
{
    private string baseUrl = "http://127.0.0.1:8080";

    [Header("UI - Autenticación")]
    public TMP_InputField inputUsername;
    public TMP_InputField inputPassword;
    public TMP_InputField inputNuevaPassword; 
    public Button botonLogin;
    public Button botonRegistro;
    public Button botonRecuperar; 

    [Header("UI - Funciones Laborales")]
    public Button botonHorarios; 
    public Button botonFichar;   
    public TextMeshProUGUI textoMensaje;

    [System.Serializable]
    public class DatosUsuario { public string username; public string password; }

    [System.Serializable]
    public class DatosRecuperacion { public string username; public string newPassword; }

    [System.Serializable]
    public class DatosJornada { public string username; public float horas; public string actividad; }

    [System.Serializable]
    public class RespuestaServidor { public bool success; public string mensaje; }

    void Start()
    {
        textoMensaje.text = "";

        // Enlazamos todos los botones a sus funciones
        if (botonLogin != null) botonLogin.onClick.AddListener(IntentarLogin);
        if (botonRegistro != null) botonRegistro.onClick.AddListener(IntentarRegistro);
        if (botonRecuperar != null) botonRecuperar.onClick.AddListener(RecuperarPassword);
        if (botonHorarios != null) botonHorarios.onClick.AddListener(ConsultarHorarios);
        if (botonFichar != null) botonFichar.onClick.AddListener(FicharJornada);
    }

    //  RUTA 1 y 2: LOGIN Y REGISTRO 
    public void IntentarLogin() => StartCoroutine(EnviarLogin("/inicioSesion", inputUsername.text, inputPassword.text, true));
    public void IntentarRegistro() => StartCoroutine(EnviarLogin("/registro", inputUsername.text, inputPassword.text, false));

    // RUTA 3: RECUPERAR/EDITAR 
    public void RecuperarPassword() => StartCoroutine(EnviarRecuperacion("/recuperarPassword", inputUsername.text, inputNuevaPassword.text));

    // RUTA 4: CONSULTAR HORARIOS 
    public void ConsultarHorarios()
    {
        MostrarMensaje("Cargando horarios...", Color.white);
        StartCoroutine(GetRequest("/horarios"));
    }

    // RUTA 5: FICHAR JORNADA 
    public void FicharJornada()
    {
        DatosJornada jornada = new DatosJornada {
            username = inputUsername.text,
            horas = 8.0f,
            actividad = "Turno Mañana"
        };
        string json = JsonUtility.ToJson(jornada);
        StartCoroutine(PostRequest("/registrarJornada", json));
    }

    // MÉTODOS DE ENVÍO
    IEnumerator EnviarLogin(string ruta, string user, string pass, bool esLogin)
    {
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass)) {
            MostrarMensaje("Faltan datos", Color.yellow); yield break;
        }
        string json = JsonUtility.ToJson(new DatosUsuario { username = user, password = pass });
        yield return PostRequest(ruta, json, esLogin);
    }

    IEnumerator EnviarRecuperacion(string ruta, string user, string newPass)
    {
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(newPass)) {
            MostrarMensaje("Faltan datos", Color.yellow); yield break;
        }
        string json = JsonUtility.ToJson(new DatosRecuperacion { username = user, newPassword = newPass });
        yield return PostRequest(ruta, json, false);
    }

    IEnumerator PostRequest(string ruta, string json, bool cargarEscena = false)
    {
        using (UnityWebRequest request = new UnityWebRequest(baseUrl + ruta, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            ProcesarRespuesta(request, cargarEscena);
        }
    }

    IEnumerator GetRequest(string ruta)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl + ruta))
        {
            yield return request.SendWebRequest();
            ProcesarRespuesta(request, false);
        }
    }

    void ProcesarRespuesta(UnityWebRequest request, bool cargarEscena)
    {
        RespuestaServidor resp = JsonUtility.FromJson<RespuestaServidor>(request.downloadHandler.text);
        if (request.result != UnityWebRequest.Result.Success) {
            MostrarMensaje(resp != null ? resp.mensaje : "Error", Color.red);
        } else {
            MostrarMensaje(resp.mensaje, Color.green);
            if (cargarEscena && resp.success) StartCoroutine(CambiarEscena());
        }
    }

    IEnumerator CambiarEscena() { yield return new WaitForSeconds(1.5f); SceneManager.LoadScene("Home"); }
    void MostrarMensaje(string msg, Color col) { textoMensaje.text = msg; textoMensaje.color = col; }
}*/