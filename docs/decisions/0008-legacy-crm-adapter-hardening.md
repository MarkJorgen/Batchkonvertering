# 0008 - Legacy CRM-adapter hardening i piloten

Pilotens næste step er at gøre den reelle ContactBLL-adapter mere robust og mere forklarende ved fejl.

Tiltag:
- tydelig trinvis logging omkring CRM-opkobling og de to ContactBLL-kald
- mere forklarende fejltekster ved exceptions i legacy-laget
- ingen ændring i domæneadfærd; kun bedre diagnoser og mere robust pilotkørsel
