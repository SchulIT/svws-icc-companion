# SVWS ICC Importer
Tool für den Import aus dem SVWS Server ins ICC.

## Migration von SchILD 2 zum SVWS-Server

Migriert man im laufenden Schuljahr von SchILD zum SVWS-Server müssen folgende Dinge beachtet werden.

1. Externe ID für Lehrkräfte: anstatt dem Kürzel wird fortan die ID in der SVWS-Datenbank verwendet. In der ICC-Datenbank muss daher für alle Lehrkräfte die "Externe ID" auf die ID in der SVWS-Datenbank gesetzt werden. Dieser Prozess lässt sich durchaus automatisieren.

2. Externe ID für Lernplattformen: falls man bereits Lernplattformen online angelegt hat, müssen diese auch in SchILD angelegt werden und die Spalte "Externe ID" muss der ID in der SVWS-Datenbank entsprechen.

