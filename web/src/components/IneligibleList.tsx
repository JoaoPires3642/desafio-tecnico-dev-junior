import type { IneligibleTeacher } from '../types'

interface Props {
  teachers: IneligibleTeacher[]
}

export function IneligibleList({ teachers }: Props) {
  if (teachers.length === 0) {
    return null
  }

  return (
    <ul className="ineligible-list">
      {teachers.map((teacher) => (
        <li key={teacher.id} className="ineligible">
          <span>{teacher.name}</span>
          <span className="reason">{teacher.reason}</span>
        </li>
      ))}
    </ul>
  )
}
