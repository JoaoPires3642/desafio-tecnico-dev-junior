import type {
  IneligibleTeacher,
  IneligibleTeacherDto,
  SearchOutcome,
  SearchRequest,
  SearchRequestDto,
  SearchResponseDto,
  TeacherMatch,
  TeacherMatchDto,
  ValidationErrorDto,
} from './types'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:8080'

/** Erro de validação devolvido pela API (HTTP 400), com a lista de mensagens. */
export class ValidationError extends Error {
  readonly messages: string[]

  constructor(messages: string[]) {
    super(messages.join(' '))
    this.name = 'ValidationError'
    this.messages = messages
  }
}

export async function searchTeachers(request: SearchRequest): Promise<SearchOutcome> {
  const payload: SearchRequestDto = {
    idioma: request.language,
    dias: request.days,
    inicioFaixa: request.startWindow,
    fimFaixa: request.endWindow,
    duracaoMinutos: request.durationMinutes,
  }

  let response: Response
  try {
    response = await fetch(`${API_URL}/api/professores/busca`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    })
  } catch {
    throw new Error('Não foi possível falar com a API. Confira se ela está no ar.')
  }

  if (response.status === 400) {
    const body = (await response.json()) as ValidationErrorDto
    throw new ValidationError(body.erros?.length ? body.erros : ['Dados inválidos.'])
  }

  if (!response.ok) {
    throw new Error(`A API respondeu ${response.status}. Tente novamente.`)
  }

  const body = (await response.json()) as SearchResponseDto
  return {
    teachers: body.professores.map(toTeacherMatch),
    ineligible: body.naoElegiveis.map(toIneligibleTeacher),
  }
}

function toTeacherMatch(dto: TeacherMatchDto): TeacherMatch {
  return {
    id: dto.id,
    name: dto.nome,
    classesPerWeek: dto.aulasNaSemana,
    availableSlots: dto.horarios,
  }
}

function toIneligibleTeacher(dto: IneligibleTeacherDto): IneligibleTeacher {
  return {
    id: dto.id,
    name: dto.nome,
    reason: dto.motivo,
  }
}
