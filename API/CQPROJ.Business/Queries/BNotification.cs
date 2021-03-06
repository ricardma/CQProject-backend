﻿using CQPROJ.Business.Entities;
using CQPROJ.Data.DB.Models;
using System;
using System.Data.Entity;
using System.Linq;

namespace CQPROJ.Business.Queries
{
    public class BNotification
    {
        public static Object GetSentNotifications(int userID, int pageID)
        {
            try
            {
                using (var db = new DBContextModel())
                {
                    var notifications = db.TblNotifications
                    .ToList()
                    .Where(x => x.UserFK == userID)
                    .OrderByDescending(x => x.ID)
                    .Skip(50 * pageID)
                    .Take(50);
                    if (notifications.Count() == 0) { return null; }
                    return notifications;
                }
            }
            catch (Exception) { return null; }
        }

        public static int GetUnreadCount(int userID)
        {
            try
            {
                using (var db = new DBContextModel())
                {
                    return db.TblValidations.Where(x => x.ReceiverFK == userID && x.Read == false).Count();
                }
            }
            catch (Exception) { return 0; }
        }

        public static Object GetReceivedNotifications(int pageID, int userID)
        {
            try
            {
                using (var db = new DBContextModel())
                {
                    var validations = db.TblValidations
                    .Where(x => x.ReceiverFK == userID)
                    .OrderByDescending(x => x.NotificationFK)
                    .Skip(50 * pageID)
                    .Take(50)
                    .ToList();
                    if (validations.Count() == 0) { return null; }
                    return validations;
                }
            }
            catch (Exception) { return null; }
        }

        public static Object GetValidationsByNotification(int notifID)
        {
            try
            {
                using (var db = new DBContextModel())
                {
                    var validations = db.TblValidations.Where(x => x.NotificationFK == notifID).ToList();
                    if (validations.Count() == 0) { return null; }
                    return validations;
                }
            }
            catch (Exception) { return null; }
        }

        public static Object GetNotification(int notifID)
        {
            try
            {
                using (var db = new DBContextModel())
                {
                    return db.TblNotifications.Find(notifID);
                }
            }
            catch (Exception) { return null; }
        }

        public static Boolean SendNotificationToUser(NotificationUser notification, int userID)
        {
            try
            {
                using (var db = new DBContextModel())
                {
                    TblNotifications notif = new TblNotifications
                    {
                        Description = notification.Description,
                        Hour = DateTime.Now,
                        Subject = notification.Subject,
                        Urgency = notification.Urgency,
                        Approval = notification.Approval,
                        UserFK = notification.SenderFK
                    };
                    db.TblNotifications.Add(notif);
                    db.SaveChanges();

                    TblValidations valid = new TblValidations
                    {
                        NotificationFK = notif.ID,
                        ReceiverFK = notification.ReceiverFK,
                        Accepted = false,
                        Read = false
                    };
                    db.TblValidations.Add(valid);
                    db.SaveChanges();

                    BAction.SetActionToUser(String.Format("enviou uma notificação ao utilizador '{0}'", db.TblUsers.Find(notification.ReceiverFK).Name), userID);
                    return true;
                }
            }
            catch (Exception) { return false; }
        }

        public static Boolean SendNotificationToClass(NotificationClass notification,int userID)
        {
            try
            {
                using (var db = new DBContextModel())
                {
                    TblNotifications notif = new TblNotifications
                    {
                        Description = notification.Description,
                        Hour = DateTime.Now,
                        Subject = notification.Subject,
                        Urgency = notification.Urgency,
                        Approval = notification.Approval,
                        UserFK = notification.SenderFK
                    };
                    db.TblNotifications.Add(notif);
                    db.SaveChanges();

                    var students = BClass.GetStudentsByClass(notification.ClassFK);
                    foreach (var student in students)
                    {
                        TblValidations valid = new TblValidations
                        {
                            ReceiverFK = BParenting.GetGuardians(student).FirstOrDefault(),
                            StudentFK = student,
                            Accepted = false,
                            Read = false
                        };
                        db.TblValidations.Add(valid);
                        db.SaveChanges();
                    }

                    var cla = db.TblClasses.Find(notification.ClassFK);
                    BAction.SetActionToUser(String.Format("enviou uma notificação a turma '{0}'", cla.Year+cla.ClassDesc), userID);
                    return true;
                }
            }
            catch (Exception) { return false; }
        }

        public static Boolean ReadNotification(int notifID, int userID)
        {
            try
            {
                using (var db = new DBContextModel())
                {
                    var valid = db.TblValidations.Find(userID, notifID);
                    valid.Read = true;
                    db.Entry(valid).State = EntityState.Modified;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception) { return false; }
        }

        public static Boolean AcceptNotification(int notifID, int userID)
        {
            try
            {
                using (var db = new DBContextModel())
                {
                    var valid = db.TblValidations.Find(userID, notifID);
                    valid.Accepted = true;
                    db.Entry(valid).State = EntityState.Modified;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception) { return false; }
        }

    }
}
