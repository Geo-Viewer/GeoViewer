using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using GeoViewer.Controller.Util;
using GeoViewer.Model.Globe;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeoViewer.View.UI.Dialogue
{
    /// <summary>
    /// A dialogue which triggers an object import when completed.
    /// </summary>
    public class ObjectImportDialogue : Dialogue<ObjectImportDialogueResponse>
    {
        [SerializeField] private VisualTreeAsset uiDocument;
        private VisualElement _instance;
        private VisualElement _ecefSelection;
        private VisualElement _llaSelection;
        private Button _cancel;
        private Button _confirm;
        private Button _coordinatesButton;
        private Button _objectButton;
        private Button _llaButton;
        private Button _ecefButton;
        private TextField _coordinatesField;
        private TextField _objectField;
        private TextField _latitudeField;
        private TextField _longitudeField;
        private TextField _altitudeField;
        private string _lastUsedFolder;
        private const string ModelPathExtension = "obj";
        private const string CoordinatesPathExtension = "txt";
        private bool _useLla;
        private bool _isOpen;
        private TaskCompletionSource<DialogueResponse<ObjectImportDialogueResponse>> _dialogueFinished;

        private void Awake()
        {
            _instance = uiDocument.Instantiate();
            _ecefSelection = _instance.Q("EcefSelection");
            _llaSelection = _instance.Q("LlaSelection");
            _objectButton = _instance.Q("objectButton") as Button;
            _coordinatesButton = _instance.Q("coordinatesButton") as Button;
            _llaButton = _instance.Q("llaButton") as Button;
            _ecefButton = _instance.Q("ecefButton") as Button;
            _objectField = _instance.Q("objectField") as TextField;
            _coordinatesField = _instance.Q("coordinatesField") as TextField;
            _latitudeField = _instance.Q("latitudeField") as TextField;
            _longitudeField = _instance.Q("longitudeField") as TextField;
            _altitudeField = _instance.Q("altitudeField") as TextField;
            _cancel = _instance.Q("cancel") as Button;
            _confirm = _instance.Q("confirm") as Button;
        }

        private void Start()
        {
            GetRoot().Add(_instance);
            _instance.visible = false;
            _lastUsedFolder = Application.dataPath;
        }

        /// <summary>
        /// Opens the import menu if it wasn't already open.
        /// </summary>
        public override async Task<DialogueResponse<ObjectImportDialogueResponse>> Open()
        {
            if (_isOpen)
            {
                return DialogueResponse<ObjectImportDialogueResponse>.Canceled();
            }

            _isOpen = true;

            _dialogueFinished = new TaskCompletionSource<DialogueResponse<ObjectImportDialogueResponse>>();
            _instance.visible = true;
            _objectButton.clicked += ObjectImport;
            _coordinatesButton.clicked += CoordinatesImport;
            _ecefButton.clicked += SetToEcefMode;
            _llaButton.clicked += SetToLlaMode;
            _confirm.clicked += Confirm;
            _cancel.clicked += Cancel;
            var result = await _dialogueFinished.Task;
            _objectButton.clicked -= ObjectImport;
            _coordinatesButton.clicked -= CoordinatesImport;
            _ecefButton.clicked -= SetToEcefMode;
            _llaButton.clicked -= SetToLlaMode;
            _confirm.clicked -= Confirm;
            _cancel.clicked -= Cancel;
            _instance.visible = false;

            _isOpen = false;
            return result;
        }

        private void GetPath(string dialogueName, string extension, TextField field)
        {
            FileBrowser.SetFilters(false, extension);
            FileBrowser.ShowLoadDialog((paths) => SetPath(field, paths), () => Debug.Log("Canceled File Browser"), FileBrowser.PickMode.Files, false, _lastUsedFolder, "", dialogueName);
        }

        private void SetPath(TextField field, string[] path)
        {
            if (path != null && path.Length != 0)
            {
                field.value = path[0];
                _lastUsedFolder = Path.GetDirectoryName(path[0]);
            }
        }

        private void SetLocationMode(bool useLla)
        {
            _useLla = useLla;
            if (_useLla)
            {
                _ecefSelection.style.display = DisplayStyle.None;
                _llaSelection.style.display = DisplayStyle.Flex;
            }
            else
            {
                _ecefSelection.style.display = DisplayStyle.Flex;
                _llaSelection.style.display = DisplayStyle.None;
            }
        }

        private void SetToLlaMode()
        {
            SetLocationMode(true);
            if (!TryGetFile(_coordinatesField.value, CoordinatesPathExtension))
            {
                return;
            }

            var point = EcefReader.ReadEcefFromFile(_coordinatesField.value).ToGlobePoint();
            _latitudeField.value = point.Latitude.ToString(CultureInfo.InvariantCulture);
            _longitudeField.value = point.Longitude.ToString(CultureInfo.InvariantCulture);
            _altitudeField.value = point.Altitude.ToString(CultureInfo.InvariantCulture);
        }

        private void SetToEcefMode()
        {
            SetLocationMode(false);
        }

        private void ObjectImport()
        {
            GetPath("Choose object path", ModelPathExtension, _objectField);
        }

        private void CoordinatesImport()
        {
            GetPath("Choose coordinates path", CoordinatesPathExtension, _coordinatesField);
        }

        private void Confirm()
        {
            var obj = TryGetFile(_objectField.value, ModelPathExtension);

            GlobePoint? point = null;
            if (!_useLla && TryGetFile(_coordinatesField.value, CoordinatesPathExtension))
            {
                point = EcefReader.ReadEcefFromFile(_coordinatesField.value).ToGlobePoint();
            }
            else if (_useLla && _latitudeField.value != "" && _longitudeField.value != "" && _altitudeField.value != "")
            {
                try
                {
                    point = new GlobePoint(double.Parse(_latitudeField.value, CultureInfo.InvariantCulture),
                        double.Parse(_longitudeField.value, CultureInfo.InvariantCulture),
                        double.Parse(_altitudeField.value, CultureInfo.InvariantCulture));
                }
                catch (ArgumentException e)
                {
                    Debug.LogWarning(e.Message);
                    point = null;
                }
                catch (FormatException)
                {
                    Debug.LogWarning("Invalid number format");
                    point = null;
                }
            }

            if (obj || point != null)
            {
                _dialogueFinished.SetResult(DialogueResponse<ObjectImportDialogueResponse>.Success(
                    new ObjectImportDialogueResponse(obj ? _objectField.value : null, point)));
            }

            if (!obj)
            {
                Debug.Log("No valid object path was set.");
            }

            if (point == null)
            {
                Debug.Log("No valid coordinate path was set.");
            }
        }

        private bool TryGetFile(string path, string extension)
        {
            return File.Exists(path) && Path.GetExtension(path) ==
                "." + extension;
        }

        private void Cancel()
        {
            _dialogueFinished.SetResult(DialogueResponse<ObjectImportDialogueResponse>.Canceled());
        }
    }
}