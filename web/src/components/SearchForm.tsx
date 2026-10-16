import { useState, type FormEvent } from 'react'
import { DURATIONS, LANGUAGES, WEEKDAYS } from '../constants'
import type { SearchRequest } from '../types'

interface Props {
  isLoading: boolean
  onSubmit: (request: SearchRequest) => void
}

export function SearchForm({ isLoading, onSubmit }: Props) {
  const [language, setLanguage] = useState<string>('Espanhol')
  const [days, setDays] = useState<string[]>(['segunda'])
  const [startWindow, setStartWindow] = useState('18:00')
  const [endWindow, setEndWindow] = useState('20:00')
  const [durationMinutes, setDurationMinutes] = useState(60)

  function toggleDay(day: string) {
    setDays((current) =>
      current.includes(day) ? current.filter((item) => item !== day) : [...current, day],
    )
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    onSubmit({ language, days, startWindow, endWindow, durationMinutes })
  }

  return (
    <form className="card form" onSubmit={handleSubmit} noValidate>
      <div className="field">
        <label htmlFor="idioma">Idioma</label>
        <select id="idioma" value={language} onChange={(event) => setLanguage(event.target.value)}>
          {LANGUAGES.map((option) => (
            <option key={option} value={option}>
              {option}
            </option>
          ))}
        </select>
      </div>

      <fieldset className="field days">
        <legend>Dias da semana</legend>
        {WEEKDAYS.map((day) => (
          <label key={day.value} className="day">
            <input
              type="checkbox"
              checked={days.includes(day.value)}
              onChange={() => toggleDay(day.value)}
            />
            {day.label}
          </label>
        ))}
      </fieldset>

      <div className="row">
        <div className="field">
          <label htmlFor="inicioFaixa">Início da faixa</label>
          <input
            id="inicioFaixa"
            type="time"
            value={startWindow}
            onChange={(event) => setStartWindow(event.target.value)}
          />
        </div>

        <div className="field">
          <label htmlFor="fimFaixa">Fim da faixa</label>
          <input
            id="fimFaixa"
            type="time"
            value={endWindow}
            onChange={(event) => setEndWindow(event.target.value)}
          />
        </div>

        <div className="field">
          <label htmlFor="duracao">Duração</label>
          <select
            id="duracao"
            value={durationMinutes}
            onChange={(event) => setDurationMinutes(Number(event.target.value))}
          >
            {DURATIONS.map((duration) => (
              <option key={duration} value={duration}>
                {duration} min
              </option>
            ))}
          </select>
        </div>
      </div>

      <button type="submit" disabled={isLoading}>
        {isLoading ? 'Buscando...' : 'Buscar professores'}
      </button>
    </form>
  )
}
