public enum StatusLevelList
{
  Win,
  Fail,
  TimeDone,
  _ // �������� ��� ���������� ������
}

public enum StatusQuestionList
{
  Win,
  Fail,
  TimeDone
}

public enum TagsList
{
  // ���� ��� �������� � �������
  QuestionStart,          // ������ ������ �������
  QuestionEnd,            // ����� ������ �������
  MemorizePhaseStart,     // ������ ���� �����������
  MemorizePhaseEnd,       // ����� ���� �����������
  ForgetPhaseStart,       // ������ ���� ����������� (����� ������ ������)
  ForgetPhaseEnd,         // ����� ���� ����������� (����� ������ ������)
  AnswerPhaseStart,       // ������ ���� ������
  AnswerPhaseEnd,         // ����� ���� ������
  AnswerSubmitted,        // ������������ �������� �����

  // ���� ��� ��������� �������
  GameStart,              // ������ ����
  GameEnd,                // ����� ����
  LevelStart,             // ������ ������
  LevelEnd,               // ����� ������
  PauseStart,             // ���� ��������������
  PauseEnd,               // ���� ����������
}

