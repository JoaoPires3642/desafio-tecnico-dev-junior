import { useState } from 'react'
import { searchTeachers, ValidationError } from './api'
import { IneligibleList } from './components/IneligibleList'
import { SearchForm } from './components/SearchForm'
import { TeacherList } from './components/TeacherList'
import type { SearchOutcome, SearchRequest } from './types'
import './App.css'

type Status = 'idle' | 'loading' | 'done' | 'failed'

export default function App() {
  const [status, setStatus] = useState<Status>('idle')
  const [outcome, setOutcome] = useState<SearchOutcome | null>(null)
  const [validationMessages, setValidationMessages] = useState<string[]>([])
  const [failureMessage, setFailureMessage] = useState<string | null>(null)

  async function handleSearch(request: SearchRequest) {
    setStatus('loading')
    setValidationMessages([])
    setFailureMessage(null)

    try {
      setOutcome(await searchTeachers(request))
      setStatus('done')
    } catch (error) {
      if (error instanceof ValidationError) {
        setValidationMessages(error.messages)
      } else {
        setFailureMessage(error instanceof Error ? error.message : 'Erro inesperado ao buscar.')
      }
      setStatus('failed')
    }
  }

  return (
    <main className="app">
      <header className="app__header">
        <h1>Encontre um professor</h1>
        <p>Informe o idioma, os dias e a faixa de horário para ver quem pode dar aula.</p>
      </header>

      <SearchForm isLoading={status === 'loading'} onSubmit={handleSearch} />

      {status === 'loading' && (
        <p className="status" role="status">
          Buscando professores...
        </p>
      )}

      {status === 'failed' && validationMessages.length > 0 && (
        <section className="card feedback feedback--error" role="alert">
          <h2>Confira os dados informados</h2>
          <ul>
            {validationMessages.map((message) => (
              <li key={message}>{message}</li>
            ))}
          </ul>
        </section>
      )}

      {status === 'failed' && failureMessage !== null && (
        <section className="card feedback feedback--error" role="alert">
          <h2>Falha na busca</h2>
          <p>{failureMessage}</p>
        </section>
      )}

      {status === 'done' && outcome !== null && (
        <>
          <section className="card">
            <h2>Professores disponíveis</h2>
            <TeacherList teachers={outcome.teachers} />
          </section>

          {outcome.ineligible.length > 0 && (
            <section className="card">
              <h2>Ficaram de fora</h2>
              <IneligibleList teachers={outcome.ineligible} />
            </section>
          )}
        </>
      )}
    </main>
  )
}
