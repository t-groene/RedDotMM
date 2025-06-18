using Microsoft.EntityFrameworkCore;
using RedDotMM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace RedDotMM.Win.Model
{
    public abstract class DataModelBaseViewModel<T> : BaseViewModel where T : class, IValidatableObject, new()
        {



        private T? _DatenObjekt;

        public T? DatenObjekt
        {
            get => _DatenObjekt;
            set
            {
                if (_DatenObjekt != value)
                {
                    _DatenObjekt = value;
                    OnPropertyChanged(nameof(DatenObjekt));
                }
            }
        }

        public event EventHandler<T>? UpdateValueEvent;



        


        public DataModelBaseViewModel(T? datenObjekt = null) : base(canSave: true, canNew: true, canDelete: false)
        {
            if (datenObjekt == null)
            {
                DatenObjekt = new T();
            }
            else
            {

                var entityType = Context.Model.FindEntityType(datenObjekt.GetType());
                if(entityType == null)
                {                   
                    // If the entity type is not found, create a new instance
                    DatenObjekt = new T();
                    return;
                }
                var primaryKey = entityType.FindPrimaryKey();
                var keyProperty = primaryKey.Properties.First();
                var entityId = keyProperty.PropertyInfo.GetValue(datenObjekt);

                if (entityId != null)
                {
                    var d = Context.Find(typeof(T), entityId);
                    if (d != null)
                    {
                        DatenObjekt = (T)d;
                    }
                    else
                    {
                        // If the entity is not found, create a new instance
                        DatenObjekt = new T();
                    }
                }
            }
        }


        /// <summary>
        /// Überprüft das DatenObjekt auf Gültigkeit. Wenn es ungültig ist, kann eine Fehlermeldung angezeigt werden, wenn showMessage auf true gesetzt ist.
        /// Ist das DatenObjekt ==null, wird ebenfalls true zurückgegeben.
        /// </summary>
        /// <param name="showMessage">Gibt an, ob eine Fehlemeldung in Form einer MessageBox angezeigt werden soll.</param>
        /// <returns>True, wenn null oder Validierung OK. False, wenn Validierung nicht OK.</returns>
        public bool ValidateDatenObjekt(bool showMessage=true)
        {

            Logging.Logger.Instance.Log($"Validiere DatenObjekt vom Typ {typeof(T).Name}.",Logging.LogType.Info);

            if (DatenObjekt != null)
            {
                ValidationContext validContext = new ValidationContext(DatenObjekt, serviceProvider: null, items: null);
                var validationResults = DatenObjekt.Validate(validContext);
                if (validationResults.Any())
                {
                    if (showMessage)
                    {
                        string errorMessage = string.Join(Environment.NewLine, validationResults.Select(v => v.ErrorMessage));
                        MessageBox.Show(errorMessage, "Validierungsfehler", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return true;
            
        }



        /// <summary>
        /// Erstellt ein neues Element vom Typ T . Wenn das aktuelle DatenObjekt nicht gespeichert ist, wird der Benutzer gefragt, ob er die Daten speichern möchte.
        /// </summary>
        public override void NeuesElement()
        {
            try
            {
                if (DatenObjekt != null)
                {
                   
                    if (ValidateDatenObjekt(false))
                    {

                        if (Context.Entry(DatenObjekt).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                        {
                            if (MessageBox.Show("Daten sind nicht gespeichert. Möchten Sie die Daten speichern?", "Daten speichern", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                Context.Add(DatenObjekt);
                                Context.SaveChanges();
                               
                                UpdateValueEvent?.Invoke(this, DatenObjekt );
                            }
                        }

                        if (Context.Entry(DatenObjekt).State == Microsoft.EntityFrameworkCore.EntityState.Modified)
                        {
                            if (MessageBox.Show("Daten wurden geändert. Möchten Sie die Änderungen speichern?", "Daten speichern", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                Context.Update(DatenObjekt);
                                Context.SaveChanges();
                                UpdateValueEvent?.Invoke(this, DatenObjekt);
                            }
                        }


                    }

                }
                // Create a new Wettbewerb instance for the next entry
                DatenObjekt = new T();




            }
            catch (Exception ex)
            {
                // Handle exception (e.g., log it)
                MessageBox.Show($"Fehler beim Erstellen neuer Daten : {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }




        public override void Loeschen()
        {
            throw new NotImplementedException();
        }

        public override void Speichern()
        {
            try
            {
                if(DatenObjekt == null)
                {
                    MessageBox.Show("Datenobjekt ist null. Bitte erstellen Sie ein neues Objekt.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                if (ValidateDatenObjekt(true))
                {

                    if (Context.Entry(DatenObjekt).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                    {
                        Context.Add(DatenObjekt);
                        Context.SaveChanges();
                        UpdateValueEvent?.Invoke(this, DatenObjekt);

                    }

                    if (Context.Entry(DatenObjekt).State == Microsoft.EntityFrameworkCore.EntityState.Modified)
                    {

                        Context.Update(DatenObjekt);
                        Context.SaveChanges();
                        UpdateValueEvent?.Invoke(this, DatenObjekt);

                    }


                }
            }
            catch (Exception ex)
            {
                // Handle exception (e.g., log it)
                MessageBox.Show($"Fehler beim Speichern : {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        public override void Schliessen()
        {
            try
            {
                if (DatenObjekt != null)
                {

                    if (ValidateDatenObjekt(false))
                    {

                        if (Context.Entry(DatenObjekt).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                        {
                            if (MessageBox.Show("Daten sind nicht gespeichert. Möchten Sie die Daten speichern?", "Daten speichern", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                Context.Add(DatenObjekt);
                                Context.SaveChanges();
                                UpdateValueEvent?.Invoke(this, DatenObjekt);
                            }
                        }

                        if (Context.Entry(DatenObjekt).State == Microsoft.EntityFrameworkCore.EntityState.Modified)
                        {
                            if (MessageBox.Show("Daten wurden geändert. Möchten Sie die Änderungen speichern?", "Daten speichern", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                Context.Update(DatenObjekt);
                                Context.SaveChanges();
                                UpdateValueEvent?.Invoke(this, DatenObjekt);
                            }
                        }


                    }

                }

                this.Context.Dispose();

                OnSchließenRequested();


            }
            catch (Exception ex)
            {
                // Handle exception (e.g., log it)
                MessageBox.Show($"Fehler beim Schließen : {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }





    }
}
