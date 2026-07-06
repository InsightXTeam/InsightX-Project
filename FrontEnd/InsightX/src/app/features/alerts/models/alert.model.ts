export type AlertType = 'Anomaly' | 'MonthlyReminder';

export interface Alert {
  id: number;
  kpiName: string;
  currentValue: number;
  threshold: number;
  message: string;
  recommendation: string;
  seenByOwner: boolean;
  createdAt: string;      // ISO date string
  alertType: AlertType;
}

/** Derived display metadata for a single alert */
export interface AlertDisplay extends Alert {
  badge: 'CRITICAL' | 'WARNING' | 'SEEN' | 'REMINDER';
  badgeClass: string;
  borderClass: string;
  formattedDate: string;
}
