import type { TeacherMatch } from '../types'

interface Props {
  teachers: TeacherMatch[]
}

export function TeacherList({ teachers }: Props) {
  if (teachers.length === 0) {
    return (
      <p className="empty" role="status">
        Nenhum professor encontrado para esse pedido.
      </p>
    )
  }

  return (
    <ul className="teacher-list">
      {teachers.map((teacher) => (
        <li key={teacher.id} className="teacher">
          <div className="teacher__head">
            <strong>{teacher.name}</strong>
            <span className="teacher__classes">
              {teacher.classesPerWeek}{' '}
              {teacher.classesPerWeek === 1 ? 'aula na semana' : 'aulas na semana'}
            </span>
          </div>
          <div className="slots">
            {teacher.availableSlots.map((slot) => (
              <span key={slot} className="slot">
                {slot}
              </span>
            ))}
          </div>
        </li>
      ))}
    </ul>
  )
}
