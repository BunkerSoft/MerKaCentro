# Skill: code-review

<skill-name>
code-review
</skill-name>

<skill-description>
Ejecuta una revisión de código completa actuando como Senior Developer.
Evalúa: Principios SOLID, Clean Code, Performance y Seguridad OWASP.
Genera un reporte con sugerencias priorizadas.
</skill-description>

<command-name>
code-review
</command-name>

<prompt>
Actúa como un Senior Developer realizando una revisión de código exhaustiva del proyecto actual.

## Pasos a seguir:

1. **Identificar archivos modificados recientemente** usando `git diff` o revisando la estructura del proyecto.

2. **Evaluar cada archivo según estos criterios:**

### Principios SOLID
- **S**ingle Responsibility: ¿Cada clase tiene una única razón para cambiar?
- **O**pen/Closed: ¿Las entidades están abiertas para extensión pero cerradas para modificación?
- **L**iskov Substitution: ¿Las clases derivadas son sustituibles por sus clases base?
- **I**nterface Segregation: ¿Las interfaces son específicas y no fuerzan implementaciones innecesarias?
- **D**ependency Inversion: ¿Se depende de abstracciones y no de implementaciones concretas?

### Clean Code
- Nombres descriptivos y significativos
- Funciones pequeñas con un solo nivel de abstracción
- Sin código duplicado (DRY)
- Sin comentarios innecesarios (el código debe ser auto-explicativo)
- Formato consistente

### Performance
- Consultas N+1 en Entity Framework
- Uso innecesario de `ToList()` antes de operaciones LINQ
- Operaciones async/await correctas
- Índices apropiados en la base de datos
- Caching donde corresponda

### Seguridad OWASP Top 10
- SQL Injection (uso de parámetros)
- XSS (encoding de salida)
- CSRF (tokens de validación)
- Autenticación y autorización apropiadas
- Exposición de datos sensibles
- Logging de información sensible

3. **Generar reporte con este formato:**

```markdown
# Code Review Report

## Resumen Ejecutivo
[Breve descripción del estado general del código]

## Hallazgos Críticos (P0) - Deben corregirse
[Lista de issues críticos que afectan seguridad o funcionalidad]

## Hallazgos Importantes (P1) - Recomendado corregir
[Lista de issues importantes que afectan mantenibilidad o performance]

## Hallazgos Menores (P2) - Sugerencias de mejora
[Lista de sugerencias para mejorar la calidad del código]

## Archivos Revisados
[Lista de archivos revisados con su estado: ✅ Aprobado / ⚠️ Con observaciones / ❌ Requiere cambios]

## Acciones Recomendadas
[Lista priorizada de acciones a tomar]
```

4. **Aplicar correcciones automáticamente** si el usuario lo solicita, empezando por las de mayor prioridad.
</prompt>

<user_invocable>true</user_invocable>
