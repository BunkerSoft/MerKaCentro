# Skill: stage-complete

<skill-name>
stage-complete
</skill-name>

<skill-description>
Ejecuta el flujo completo de cierre de etapa: Linter -> Tests -> Code Review -> Commit.
Verifica que todos los pasos pasen antes de hacer el commit.
</skill-description>

<command-name>
stage-complete
</command-name>

<prompt>
Ejecuta el flujo completo de verificación y cierre de etapa para el proyecto MiniMarket.

## Flujo de Cierre de Etapa

### Paso 1: Linter (dotnet format)
```bash
export PATH="$HOME/.dotnet:$PATH"
cd /home/satanas/Documentos/minimarket
dotnet format --verify-no-changes
```

- Si hay errores de formato:
  1. Ejecutar `dotnet format` para corregirlos automáticamente
  2. Mostrar los archivos que fueron modificados
  3. Volver a verificar

### Paso 2: Ejecutar Tests
- Ejecutar todas las pruebas unitarias
- Verificar cobertura >= 80%
- Si falla, DETENER el flujo y reportar

### Paso 3: Code Review
- Ejecutar revisión de código completa
- Aplicar correcciones automáticas de issues P0 (críticos)
- Reportar issues P1 y P2 para revisión manual

### Paso 4: Commit (solo si los pasos anteriores pasan)
```bash
git add -A
git status
```

- Solicitar al usuario el mensaje de commit o usar el formato:
  `feat: Etapa X - [Descripción de la etapa]`
- Realizar el commit

## Formato de Reporte Final

```markdown
# Stage Complete Report

## Etapa: [Nombre de la etapa]

### 1. Linter
- Estado: ✅ Pasó / ❌ Falló
- Archivos corregidos: X

### 2. Tests
- Estado: ✅ Pasó / ❌ Falló
- Cobertura: X%
- Tests pasaron: X/Y

### 3. Code Review
- Estado: ✅ Aprobado / ⚠️ Con observaciones
- Issues P0: X (corregidos)
- Issues P1: X (pendientes)
- Issues P2: X (sugerencias)

### 4. Commit
- Estado: ✅ Completado / ❌ Pendiente
- Hash: [hash del commit]
- Mensaje: [mensaje del commit]

## Siguiente Etapa
[Indicar qué sigue en el plan de trabajo]
```

## Comportamiento en caso de fallo

- Si el Linter falla: Corregir automáticamente y reintentar
- Si los Tests fallan: DETENER y reportar tests fallidos
- Si la cobertura es < 80%: DETENER y reportar áreas sin cobertura
- Si hay issues P0 en Code Review: Corregir y reintentar tests
</prompt>

<user_invocable>true</user_invocable>
