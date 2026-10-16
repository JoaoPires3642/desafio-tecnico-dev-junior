/**
 * Contrato da API. As chaves ficam em português porque é isso que trafega no JSON
 * (mesma ideia dos JsonPropertyName no C#).
 */
export interface SearchRequestDto {
  idioma: string
  dias: string[]
  inicioFaixa: string
  fimFaixa: string
  duracaoMinutos: number
}

export interface TeacherMatchDto {
  id: number
  nome: string
  aulasNaSemana: number
  horarios: string[]
}

export interface IneligibleTeacherDto {
  id: number
  nome: string
  motivo: string
}

export interface SearchResponseDto {
  professores: TeacherMatchDto[]
  naoElegiveis: IneligibleTeacherDto[]
}

export interface ValidationErrorDto {
  mensagem: string
  erros: string[]
}

/**
 * Modelo usado pelos componentes, com identificadores em inglês.
 * A conversão fica em api.ts, junto do fetch.
 */
export interface SearchRequest {
  language: string
  days: string[]
  startWindow: string
  endWindow: string
  durationMinutes: number
}

export interface TeacherMatch {
  id: number
  name: string
  classesPerWeek: number
  availableSlots: string[]
}

export interface IneligibleTeacher {
  id: number
  name: string
  reason: string
}

export interface SearchOutcome {
  teachers: TeacherMatch[]
  ineligible: IneligibleTeacher[]
}
